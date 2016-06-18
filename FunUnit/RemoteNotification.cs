using System;

namespace FunUnit
{
    public class RemoteNotification : ConsoleFunUnitNotification,
        IFunUnitNotification
    {
        public void OnBuildError(string s)
        {
            throw new NotImplementedException();
        }

        public void OnGeneralError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void OnSuccess(string s)
        {
            throw new NotImplementedException();
        }

        public void OnFailure(string s)
        {
            throw new NotImplementedException();
        }

        public string GetSourcePath()
        {
            throw new NotImplementedException();
        }
    }
}