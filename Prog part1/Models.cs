using System;
using System.Collections.Generic;

namespace Prog_part1
{
    public class CybersecurityTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Reminder { get; set; } = ""; 
        public string Status { get; set; } = "Pending"; 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class QuizQuestion
    {
        public string QuestionText { get; set; } = "";
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
    }
}