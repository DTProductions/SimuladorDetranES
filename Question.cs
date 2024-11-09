using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetranScraper {
    internal class Question {
        public QuestionTitle Title { get; }
        public List<QuestionOption> Options { get; }

        public Question(QuestionTitle title, List<QuestionOption> options) {
            Title = title;
            Options = options;
        }
    }
}
