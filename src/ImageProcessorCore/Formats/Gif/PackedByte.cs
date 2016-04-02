﻿// <copyright file="PackedByte.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageProcessorCore.Formats
{
    using System;

    /// <summary>
    /// Represents a byte of data in a GIF data stream which contains a number
    /// of data items.
    /// TODO: Finish me.
    /// </summary>
    internal struct PackedByte
    {
        /// <summary>
        /// The individual bits representing the packed byte.
        /// </summary>
        private static readonly bool[] Bits = new bool[8];

        #region constructor( int )
        ///// <summary>
        ///// Constructor.
        ///// Sets the bits in the packed fields to the corresponding bits from
        ///// the supplied byte.
        ///// </summary>
        ///// <param name="data">
        ///// A single byte of data, consisting of fields which may be of one or
        ///// more bits.
        ///// </param>
        //public PackedByte(int data) : this(data)
        //{
        //    for (int i = 0; i < 8; i++)
        //    {
        //        var bitShift = 7 - i;
        //        var bitValue = (data >> bitShift) & 1;
        //        bool bit = bitValue == 1;
        //        _bits[i] = bit;
        //    }
        //}
        #endregion

        /// <summary>
        /// Gets the byte which represents the data items held in this instance.
        /// </summary>
        public byte Byte
        {
            get
            {
                int returnValue = 0;
                int bitShift = 7;
                foreach (bool bit in Bits)
                {
                    int bitValue;
                    if (bit)
                    {
                        bitValue = 1 << bitShift;
                    }
                    else
                    {
                        bitValue = 0;
                    }
                    returnValue |= bitValue;
                    bitShift--;
                }
                return Convert.ToByte(returnValue & 0xFF);
            }
        }

        /// <summary>
        /// Sets the specified bit within the packed fields to the supplied 
        /// value.
        /// </summary>
        /// <param name="index">
        /// The zero-based index within the packed fields of the bit to set.
        /// </param>
        /// <param name="valueToSet">
        /// The value to set the bit to.
        /// </param>
        public void SetBit(int index, bool valueToSet)
        {
            if (index < 0 || index > 7)
            {
                string message
                    = "Index must be between 0 and 7. Supplied index: "
                    + index;
                throw new ArgumentOutOfRangeException(nameof(index), message);
            }
            Bits[index] = valueToSet;
        }

        /// <summary>
        /// Sets the specified bits within the packed fields to the supplied 
        /// value.
        /// </summary>
        /// <param name="startIndex">
        /// The zero-based index within the packed fields of the first bit to 
        /// set.
        /// </param>
        /// <param name="length">
        /// The number of bits to set.
        /// </param>
        /// <param name="valueToSet">
        /// The value to set the bits to.
        /// </param>
        public void SetBits(int startIndex, int length, int valueToSet)
        {
            if (startIndex < 0 || startIndex > 7)
            {
                string message = $"Start index must be between 0 and 7. Supplied index: {startIndex}";
                throw new ArgumentOutOfRangeException(nameof(startIndex), message);
            }

            if (length < 1 || startIndex + length > 8)
            {
                string message = "Length must be greater than zero and the sum of length and start index must be less than 8. "
                                 + $"Supplied length: {length}. Supplied start index: {startIndex}";
                throw new ArgumentOutOfRangeException(nameof(length), message);
            }

            int bitShift = length - 1;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                int bitValueIfSet = (1 << bitShift);
                int bitValue = (valueToSet & bitValueIfSet);
                int bitIsSet = (bitValue >> bitShift);
                Bits[i] = (bitIsSet == 1);
                bitShift--;
            }
        }

        /// <summary>
        /// Gets the value of the specified bit within the byte.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the bit to get.
        /// </param>
        /// <returns>
        /// The value of the specified bit within the byte.
        /// </returns>
        public bool GetBit(int index)
        {
            if (index < 0 || index > 7)
            {
                string message = $"Index must be between 0 and 7. Supplied index: {index}";
                throw new ArgumentOutOfRangeException(nameof(index), message);
            }
            return Bits[index];
        }

        /// <summary>
        /// Gets the value of the specified bits within the byte.
        /// </summary>
        /// <param name="startIndex">
        /// The zero-based index of the first bit to get.
        /// </param>
        /// <param name="length">
        /// The number of bits to get.
        /// </param>
        /// <returns>
        /// The value of the specified bits within the byte.
        /// </returns>
        public int GetBits(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > 7)
            {
                string message = $"Start index must be between 0 and 7. Supplied index: {startIndex}";
                throw new ArgumentOutOfRangeException(nameof(startIndex), message);
            }

            if (length < 1 || startIndex + length > 8)
            {
                string message = "Length must be greater than zero and the sum of length and start index must be less than 8. "
                                 + $"Supplied length: {length}. Supplied start index: {startIndex}";

                throw new ArgumentOutOfRangeException(nameof(length), message);
            }

            int returnValue = 0;
            int bitShift = length - 1;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                int bitValue = (Bits[i] ? 1 : 0) << bitShift;
                returnValue += bitValue;
                bitShift--;
            }
            return returnValue;
        }
    }
}