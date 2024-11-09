﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetranScraper {
    internal class QuestionTitle {
        public string Text { get; }
        public string? ImageBase64 { get; }
        public string? ImageType { get; }

        public QuestionTitle(string text, string? imageBase64, string? imageType) {
            Text = text;
            ImageBase64= imageBase64;
            ImageType = imageType;
        }
    }
}
