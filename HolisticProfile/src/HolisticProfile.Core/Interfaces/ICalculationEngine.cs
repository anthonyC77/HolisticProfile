namespace HolisticProfile.Core.Interfaces;

public interface ICalculationEngine
{
    Models.MillmanLifePath Calculate(DateTime birthDate);
}
