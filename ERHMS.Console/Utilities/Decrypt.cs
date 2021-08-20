﻿namespace ERHMS.Console.Utilities
{
    public class Decrypt : Utility
    {
        public string Text { get; }

        public Decrypt(string text)
        {
            Text = text;
        }

        public override void Run()
        {
            Out.WriteLine(Epi.Configuration.Decrypt(Text));
        }
    }
}
