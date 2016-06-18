using System;

namespace FunUnit
{
    public class ConsoleFunUnitNotification : IFunUnitNotification
    {
        private void WriteWarn(string txt)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(txt);
            Console.ResetColor();
        }

        private void WritePass(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(txt);
            Console.ResetColor();
        }

        private void WriteFail(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(txt);
            Console.ResetColor();
        }

        public void OnBuildError(string s)
        {
            Console.WriteLine(s);
        }

        public void OnGeneralError(Exception ex)
        {
            WriteWarn(ex.Message + " - " + ex?.InnerException?.Message);
        }

        public void OnSuccess(string s)
        {
            WritePass(s);
        }

        public void OnFailure(string s)
        {
            WriteFail(s);
        }

        public string GetSourcePath()
        {
            Console.WriteLine("Enter root source file path :");
            var path = Console.ReadLine();
            Console.WriteLine("Cool. Lets start testing, yo!");
            return path;
        }
    }
}