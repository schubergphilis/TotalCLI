using System;
using System.IO;
using System.Text;
using Total.CLI;

namespace SchubergPhilis.Demo
{
    public class Demo_TotalCLI
    {
        static void Main()
        {
            // ====================================================================================================================
            //  TotalCLI-Demo
            //
            //   Demonstrate the various possibilities of TotalCLI
            // --------------------------------------------------------------------------------------------------------------------
            //cli.SetSwitchIdentifier("-");
            //cli.SetSSecretMarker('^');
            cli.AddDefinition("{secret}[string]password");
            cli.AddDefinition("{negatable,alias(verbose)}[bool]debug");
            cli.AddDefinition("{validateLength(,10)}[string]valLength");
            cli.AddDefinition("{validateCount(,2)}[string]valCount");
            cli.AddDefinition("{validateRange(-200,200)}[int]valRange");
            cli.AddDefinition("{alias(ipAddress),validatePattern(/^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$/)}[string]valPattern");
            cli.AddDefinition("{validateCount(4,4),validateRange(0,9)}[byte[]]Guess");
            cli.AddDefinition("{notnullorempty,validateset(Install|Remove|Start|Stop|Restart)}[string]service");
            cli.AddDefinition("{alias(?),validateset(Help|Definitions|Switches|AllowRules|ValueTypes|Alias|Mandatory|NotNullOrEmpty|Negatable|Secret|ValidateLength|ValidateCount|ValidateRange|ValidateSet|ValidatePattern)}[string]help=Help");
            cli.AddRule("<disallow any2(password,valCount,service,help)>");
            cli.LoadDefinitions();
            // --------------------------------------------------------------------------------------------------------------------
            //   Show help?
            // --------------------------------------------------------------------------------------------------------------------
            if (cli.IsPresent("Help")) { ShowDemoHelp(cli.GetValue("Help")); }
            // --------------------------------------------------------------------------------------------------------------------
            if (cli.IsPresent("Password")) { Console.WriteLine("Cannot keep a secret from me ;) .... {0}",cli.GetValue("Password")); }
            // --------------------------------------------------------------------------------------------------------------------

            // --------------------------------------------------------------------------------------------------------------------
        }

        // ========================================================================================================================
        //   Show the help info on the requested topic
        // ------------------------------------------------------------------------------------------------------------------------
        private static void ShowDemoHelp(string Topic)
        {
            Console.WriteLine("\n\tSchuberg Philis - TotalCLI Demo\n\n\tCopyright (C) 2015 Hans van Veen, Schuberg Philis\n");
            string valHelp = "";
            switch (Topic.ToLower())
            {
// ################################################################################################################################
                case "definitions": valHelp = @"
   The switch definition strings define which switches are available to the program and their validation rules.
   Parameter validation strings have the following format;

     cli.AddDefinition(""{validation rules}[valuetype]switchname=default-value"");
     cli.AddDefinition(""{validation rules}[valuetype]switchname=default-value"");
     cli.AddDefinition(.................);
     cli.AddRule(""<disallow any2(....,.... )>"");
  
   The {validation rules} constist of a set of directives which are applied to the switch and/or its assigned value.
   Available validation directives are; Alias, Mandatory, NotNullOrEmpty, Negatable, Secret, ValidateLength, ValidateCount,
   ValidateRange, ValidateSet and ValidatePattern. Use ""-help 'directive'"" to get more info on each of them.

   A special rule is the ""<disallow any2(....,.... )>"", see ""-help AllowRules"" for more information

   The [valuetype] specifies the switch value type (hehe...) Use ""-help ValueTypes"" to learn more about this.

   The switch name specifies the full name of the switch, this name is used to identify the switch in the
   resulting sorted list. A default value can be assigned to the switch, see ""-help Switches"".

"; break;
// ################################################################################################################################
                case "switches": valHelp = @"
   The switch name is THE key for saving and retrieving argument values. The TotalCLI function use this key
   (aka: cli.GetValue(""'switch name'"") returns the argument value for the requested switch)

   A switch can be assigned a default value (switchname=default-value). If the switch is specified on the command
   line, and no argument value is found for this switch, the default value will be used.

"; break;
// ################################################################################################################################
                case "allowrules": valHelp = @"
   Allow/Disallow rulles provide means to disallow combinations of switchs on the commandline
   
   Example: cli.AddDefinition(""[short]P1"");
            cli.AddDefinition(""[short]P2"");
            cli.AddDefinition(""[short]P3"");
            cli.AddRule(""<disallow any2(P1,P2,P3)>"");
  
   As soon as a combination of P1, P2 and/or P3 is used on the command line, an error will be displayed and the program
   will terminate. You can try ""TotalCLI_Demo.exe -? Definitions -passw"" ;-)

"; break;
// ################################################################################################################################
                case "valuetypes": valHelp = @"
   Each switch can be assigned a value type. If no value type is assigned, type [string] will be assumed.
   The value types recognized by TotalCLI are;

               [string], [bool], [byte], [sbyte], [char], [decimal], [float],
               [double], [int], [uint], [long], [ulong], [short] and [ushort]

   Using an unknown type will result in a terminating error.

"; break;
// ################################################################################################################################
                case "alias": valHelp = @"
   A switch can be assigned an alias. For example ""{alias(Verbose)}[bool]Debug""

   You can now use ""'program' -debug"" or ""'program' -verbose"", both will have the same result.
   Both will created a SortedList entry with the name Debug, and since it is a boolean it will be
   assigned the value 'true' (there must be a reason why you used this switch... so it is true!)

"; break;
// ################################################################################################################################
                case "mandatory": valHelp = @"
   A mandatory switch MUST be present on the command line, if not present and running in interactive mode,
   you will be prompted for a value. Not interactive or not providing a value will result in a terminating error.

"; break;
// ################################################################################################################################
                case "notnullorempty": valHelp = @"
   A NotNullOrEmpty switch MUST have a value, if no value is assigned and running in interactive mode,
   you will be prompted for the value. Not interactive or not providing a value will result in a terminating error.

"; break;
// ################################################################################################################################
                case "negatable": valHelp = @"
   A negatable switch can have 2 (4 if an alias is used) appearances.
   For example ""{alias(Verbose),negatable}[bool]Debug"" will result in the following possibilities;

            ""'programm' -Debug""          Results in cli.GetValue(""Debug"") = 'true'
            ""'programm' -Verbose""        Results in cli.GetValue(""Debug"") = 'true'
            ""'programm' -noDebug""        Results in cli.GetValue(""Debug"") = 'false'
            ""'programm' -noVerbose""      Results in cli.GetValue(""Debug"") = 'false'

   This directive can be used in combination with every value type, but it is only implemented for/used by [bool] types.

"; break;
// ################################################################################################################################
                case "secret": valHelp = @"
   A secret switch will use the command line (or default) value, but if no value is present it will prompt
   for it. When entering the 'secret' value, the entered characters will be displayed as an ""*"" sign.

"; break;
// ################################################################################################################################
                default:             valHelp = @"
   This demo shows the usage of and some of the possibilities of TotalCLI. This help section will show how
   TotalCLI is used in a standard C# program:

   Step 1: Define the required switches and rules
   	       cli.AddDefinition(""{notnullorempty}[string]logFile"");

   Step 2: Load the definitions
   	       cli.LoadDefinitions();

   Step 3: Use the switches in your program
           cli.IsPresent(""switch"") to see whether a switch has been specified
           cli.GetValue(""switch"") to get the switch value
           cli.IsNegated(""switch"") to see whether a negated switch has been specified

   The strength of TotalCLI is in Step 2 - loading the switch definitions.
   Use ""-help Definitions"" to learn more about creating switch definintion strings.
  
   What else is there? The validation directives! Use one of the following directives with -help and you will see.

              Definitions, Parameters, AllowRules, ValueTypes, Alias, Mandatory, NotNullOrEmpty, Negatable,
              Secret, ValidateLength, ValidateCount, ValidateRange, ValidateSet, ValidatePattern

   A very, very special switch is --cliDebug (case sensitive) - It will instruct TotalCLI to provide
   debug information if itself! Can be handy for troubleshooting.

"; break;
// ################################################################################################################################
            }
            Console.WriteLine(valHelp);
        }
        // ========================================================================================================================
        //   Show the help info on the requested topic
        // ------------------------------------------------------------------------------------------------------------------------
    }
}
