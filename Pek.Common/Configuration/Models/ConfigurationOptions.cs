using System;

namespace Pek.Configuration.Models
{
    public class ConfigurationOptions
    {
        public string Option1 { get; set; } = "DefaultValue1";
        public int Option2 { get; set; } = 10;
        public bool Option3 { get; set; } = true;

        // Add more configuration options as needed
    }
}