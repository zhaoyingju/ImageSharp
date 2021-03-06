﻿// <copyright file="TestImage.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ImageSharp.Formats;
    using ImageSharp.PixelFormats;

    using Xunit;

    /// <summary>
    /// A test image file.
    /// </summary>
    public class TestFormat : IConfigurationModule, IImageFormat
    {
        public static TestFormat GlobalTestFormat { get; } = new TestFormat();

        public static void RegisterGloablTestFormat()
        {
            Configuration.Default.Configure(GlobalTestFormat);
        }

        public TestFormat()
        {
            this.Encoder = new TestEncoder(this); ;
            this.Decoder = new TestDecoder(this); ;
        }

        public List<DecodeOperation> DecodeCalls { get; } = new List<DecodeOperation>();

        public IImageEncoder Encoder { get; }

        public IImageDecoder Decoder { get; }

        private byte[] header = Guid.NewGuid().ToByteArray();

        public MemoryStream CreateStream(byte[] marker = null)
        {
            MemoryStream ms = new MemoryStream();
            byte[] data = this.header;
            ms.Write(data, 0, data.Length);
            if (marker != null)
            {
                ms.Write(marker, 0, marker.Length);
            }
            ms.Position = 0;
            return ms;
        }

        Dictionary<Type, object> _sampleImages = new Dictionary<Type, object>();


        public void VerifyDecodeCall(byte[] marker, Configuration config)
        {
            DecodeOperation[] discovered = this.DecodeCalls.Where(x => x.IsMatch(marker, config)).ToArray();


            Assert.True(discovered.Any(), "No calls to decode on this formate with the proveded options happend");

            foreach (DecodeOperation d in discovered)
            {
                this.DecodeCalls.Remove(d);
            }
        }

        public Image<TPixel> Sample<TPixel>()
            where TPixel : struct, IPixel<TPixel>
        {
            lock (this._sampleImages)
            {
                if (!this._sampleImages.ContainsKey(typeof(TPixel)))
                {
                    this._sampleImages.Add(typeof(TPixel), new Image<TPixel>(1, 1));
                }

                return (Image<TPixel>)this._sampleImages[typeof(TPixel)];
            }
        }

        public string MimeType => "img/test";

        public string Extension => "test_ext";

        public IEnumerable<string> SupportedExtensions => new[] { "test_ext" };

        public int HeaderSize => this.header.Length;

        public string Name => this.Extension;

        public string DefaultMimeType => this.MimeType;

        public IEnumerable<string> MimeTypes => new[] { this.MimeType };

        public IEnumerable<string> FileExtensions => this.SupportedExtensions;

        public bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length < this.header.Length)
            {
                return false;
            }
            for (int i = 0; i < this.header.Length; i++)
            {
                if (header[i] != this.header[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void Configure(Configuration host)
        {
            host.AddImageFormatDetector(new TestHeader(this));
            host.SetEncoder(this, new TestEncoder(this));
            host.SetDecoder(this, new TestDecoder(this));
        }

        public struct DecodeOperation
        {
            public byte[] marker;
            internal Configuration config;

            public bool IsMatch(byte[] testMarker, Configuration config)
            {

                if (this.config != config)
                {
                    return false;
                }

                if (testMarker.Length != this.marker.Length)
                {
                    return false;
                }

                for (int i = 0; i < this.marker.Length; i++)
                {
                    if (testMarker[i] != this.marker[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public class TestHeader : IImageFormatDetector
        {

            private TestFormat testFormat;

            public int HeaderSize => testFormat.HeaderSize;

            public IImageFormat DetectFormat(ReadOnlySpan<byte> header)
            {
                if (testFormat.IsSupportedFileFormat(header))
                    return testFormat;

                return null;
            }

            public TestHeader(TestFormat testFormat)
            {
                this.testFormat = testFormat;
            }
        }
        public class TestDecoder : ImageSharp.Formats.IImageDecoder
        {
            private TestFormat testFormat;

            public TestDecoder(TestFormat testFormat)
            {
                this.testFormat = testFormat;
            }

            public IEnumerable<string> MimeTypes => new[] { testFormat.MimeType };

            public IEnumerable<string> FileExtensions => testFormat.SupportedExtensions;

            public int HeaderSize => testFormat.HeaderSize;

            public Image<TPixel> Decode<TPixel>(Configuration config, Stream stream) where TPixel : struct, IPixel<TPixel>

            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                var marker = ms.ToArray().Skip(this.testFormat.header.Length).ToArray();
                this.testFormat.DecodeCalls.Add(new DecodeOperation
                {
                    marker = marker,
                    config = config
                });

                // TODO record this happend so we an verify it.
                return this.testFormat.Sample<TPixel>();
            }

            public bool IsSupportedFileFormat(Span<byte> header) => testFormat.IsSupportedFileFormat(header);
        }

        public class TestEncoder : ImageSharp.Formats.IImageEncoder
        {
            private TestFormat testFormat;

            public TestEncoder(TestFormat testFormat)
            {
                this.testFormat = testFormat;
            }

            public IEnumerable<string> MimeTypes => new[] { testFormat.MimeType };

            public IEnumerable<string> FileExtensions => testFormat.SupportedExtensions;

            public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : struct, IPixel<TPixel>
            {
                // TODO record this happend so we an verify it.
            }
        }
    }
}
