using System;

namespace Bewit.Validation.Generation.Exceptions;

public class BewitMissingConfigurationException: Exception
{
    public string Component { get; }

    public BewitMissingConfigurationException(string component)
    {
        Component = component;
    }
}
