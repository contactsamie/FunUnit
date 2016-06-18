using System;

namespace FunUnit
{
    public interface IFunUnitNotification
    {
        void OnBuildError(string s);
        void OnGeneralError(Exception ex);
        void OnSuccess(string s);
        void OnFailure(string s);
        string GetSourcePath();
    }
}