// ###########################################################################################################################
//
//     TotalCLI ™
//          
//     Copyright © 2015, Hans L.M. van Veen
//  
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//  
//         http://www.apache.org/licenses/LICENSE-2.0
//  
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
//       
// ###########################################################################################################################


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Total.CLI
{
    public class cli
    {
        private static bool cliDefinitionsLoaded;
        private static byte cliDebugLevel = 0;
        private static char secretMark = '#';
        private static string cliSwitchIdentifier;
        private static Regex rgxAllowAny;
        private static Regex rgxDisallow;
        private struct cliDataSet
        {
            public string fullSwitchName;
            public string fullAliasName;
            public string type;
            public string argumentValue;
            public string defaultValue;
            public bool isPresent;
            public bool hasArgument;
            public bool isMandatory;
            public bool isSecret;
            public bool isNegated;
            public bool isNegatable;
            public bool notNullOrEmpty;
            public bool doValidateCount; public string[] validateCount;
            public bool doValidateLength; public string[] validateLength;
            public bool doValidatePattern; public string validatePattern;
            public bool doValidateRange; public string[] validateRange;
            public bool doValidateSet; public string validateSet;
        }
        private static void WriteDebugLevel1(string Message)
        {
            if (cliDebugLevel < 1) { return; }
            ConsoleColor bgc = Console.BackgroundColor;
            ConsoleColor fgc = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[cliDEBUG] - " + Message);
            Console.BackgroundColor = bgc;
            Console.ForegroundColor = fgc;
        }
        private static void WriteDebugLevel2(string Message)
        {
            if (cliDebugLevel < 2) { return; }
            ConsoleColor bgc = Console.BackgroundColor;
            ConsoleColor fgc = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[cliDEBUG] - " + Message);
            Console.BackgroundColor = bgc;
            Console.ForegroundColor = fgc;
        }
        private static void WriteDebugLevel3(string Message)
        {
            if (cliDebugLevel < 3) { return; }
            ConsoleColor bgc = Console.BackgroundColor;
            ConsoleColor fgc = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[cliDEBUG] - " + Message);
            Console.BackgroundColor = bgc;
            Console.ForegroundColor = fgc;
        }
        private static void WriteWarning(string Message)
        {
            ConsoleColor bgc = Console.BackgroundColor;
            ConsoleColor fgc = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[cliWARN]  - " + Message);
            Console.BackgroundColor = bgc;
            Console.ForegroundColor = fgc;
        }
        private static void WriteError(string Message)
        {
            ConsoleColor bgc = Console.BackgroundColor;
            ConsoleColor fgc = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[cliERROR] - " + Message);
            Console.BackgroundColor = bgc;
            Console.ForegroundColor = fgc;
            Environment.Exit(1);
        }
        private static string ReadPassword()
        {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ };
            var pass = new Stack<char>(); char chr = (char)0;
            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (chr == BACKSP) { if (pass.Count > 0) { Console.Write("\b \b"); pass.Pop(); } }
                else if (chr == CTRLBACKSP) { while (pass.Count > 0) { Console.Write("\b \b"); pass.Pop(); } }
                else if (FILTERED.Count(x => chr == x) > 0) { }
                else { pass.Push((char)chr); Console.Write(secretMark); }
            }
            Console.WriteLine();
            return new string(pass.Reverse().ToArray());
        }
        private static void ShowCLIdetails(cliDataSet cli)
        {
            if (cliDebugLevel < 2) { return; }
            WriteDebugLevel2(new string('=', 100));
            WriteDebugLevel2(String.Format("Switch Name: {0} [{1}])", cli.fullSwitchName, cli.fullAliasName));
            WriteDebugLevel2(new string('-', 48) + "+" + new string('-', 51));
            WriteDebugLevel2(String.Format(" default Value: {0,-31} | {1}{2}", cli.defaultValue, " argument Value: ", cli.argumentValue));
            WriteDebugLevel2(String.Format("  has Argument: {0,-31} | {1}{2}", cli.hasArgument, "   is Mandatory: ", cli.isMandatory));
            WriteDebugLevel2(String.Format("     is Secret: {0,-31} | {1}{2}", cli.isSecret, "not NullOrEmpty: ", cli.notNullOrEmpty));
            WriteDebugLevel2(String.Format("  is Negatable: {0,-31} | {1}{2}", cli.isNegatable, "     is Negated: ", cli.isNegated));
            WriteDebugLevel2(String.Format("    value Type: {0,-31} | {1}{2}", cli.type, " valid. Pattern: ", cli.validatePattern));
            string pn = String.Format("{0}/{1}", cli.validateCount[0], cli.validateCount[1]);
            string an = String.Format("{0}/{1}", cli.validateLength[0], cli.validateLength[1]);
            WriteDebugLevel2(String.Format(" valid. Length: {0,-31} | {1}{2}/{3}", an, "   valid. Range: ", cli.validateRange[0], cli.validateRange[1]));
            WriteDebugLevel2(String.Format("  valid. Count: {0,-31} | {1}{2}", pn, "     valid. Set: ", cli.validateSet));
        }
        private static void CheckDebugState()
        {
            // ================================================================================================================================================
            //  Check for the TotalCLI specific arguments like cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            if (Environment.CommandLine.Contains("--cliDebugLevel1")) { cliDebugLevel = 1; }
            if (Environment.CommandLine.Contains("--cliDebugLevel2")) { cliDebugLevel = 2; }
            if (Environment.CommandLine.Contains("--cliDebugLevel3")) { cliDebugLevel = 3; }
        }
        // ------------------------------------------------------------------------------------------------------------------------------------------------
        //   cli definiton and rule storage
        // ------------------------------------------------------------------------------------------------------------------------------------------------
        static List<string> switchList = new List<string>();
        static List<string> checkRules = new List<string>();
        static Dictionary<string, string> aliasDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static Dictionary<string, cliDataSet> switchDictionary = new Dictionary<string, cliDataSet>(StringComparer.OrdinalIgnoreCase);
        // ------------------------------------------------------------------------------------------------------------------------------------------------

        // ====================================================================================================================================================
        //   cli.SetSecretMarker is used to set the 'marker' which is used when entering a secret value
        //   TotalCLI by default uses the "#" character, but this can be altered using SetSecretMarker function.
        // ====================================================================================================================================================
        public static void SetSSecretMarker(char Marker = '#')
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Once the cli definitions have been loaded, the switch identifier can no longer be altered.
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            if (cliDefinitionsLoaded) { WriteWarning("Action ignored, secret marker already set"); }
            else { secretMark = Marker; }
            WriteDebugLevel1("Using secret marker: " + secretMark);
            return;
        }

        // ====================================================================================================================================================
        //   cli.SetSwitchIdentifier is used to set the switch identifier (huh!). Switches can be identified by "-", "--", "/", .......
        //   TotalCLI by default uses the "/" character, but this can be altered using SetSwitchIdentifier.
        // ====================================================================================================================================================
        public static void SetSwitchIdentifier(string Identifier = "/")
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Once the cli definitions have been loaded, the switch identifier can no longer be altered.
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            if (cliDefinitionsLoaded) { WriteWarning("Action ignored, switch identifier already set"); }
            else { cli.cliSwitchIdentifier = (Identifier.Length > 0) ? cli.cliSwitchIdentifier = Identifier : cli.cliSwitchIdentifier = "/"; }
            WriteDebugLevel1("Using switch identifier: " + cli.cliSwitchIdentifier);
            return;
        }

        // ====================================================================================================================================================
        //   cli.AddDefinition performs the following checks on the passed definition/rule;
        //    1 - Validate the used switch definitions (make sure no typo's have been made etc. This part creates a Dictionary using the full switch
        //        name as key, all subsequent information is stored in a structure which is attached to this key. The function quits as soon as a invalid
        //        definition is detected.
        //    2 - Determine of each switch and alias the shortest abbreviation, make sure there are no conflicts.
        // ====================================================================================================================================================
        public static void AddDefinition(string Definition)
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   String.Split charater arrays; round, curly, square and angle brackets
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            char[] rBrackets = { '(', ')' }; char[] cBrackets = { '{', '}' }; char[] sBrackets = { '[', ']' }; char[] fBrackets = { '<', '>' };
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Value type list. This is the lists of value types supported by TotalCLI
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            List<string> typeList = new List<string> { "[string]","[bool]","[byte]","[sbyte]","[char]","[decimal]","[float]",
                                                       "[double]", "[int]", "[uint]", "[long]", "[ulong]", "[short]", "[ushort]",
                                                       "[string[]]","[byte[]]","[sbyte[]]","[char[]]","[decimal[]]","[float[]]",
                                                       "[double[]]", "[int[]]", "[uint[]]", "[long[]]", "[ulong[]]", "[short[]]", "[ushort[]]" };
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Use a Regex expression to validate the passed definition and the validiation rules defined within.
            //   - Definition record:  "^(\{.*\})?(\[\w+(\[\])?\])?(\w+)(\=.*)?$"   For example: {validation}[type]switch(=default)
            //   - Validation ruiles:  a lot!
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            Regex rgxDefinition = new Regex(@"^(\{.*\})?(\[\w+(\[\])?\])?(\w+)(\=.*)?$");
            Regex rgxAlias = new Regex(@"(?i)^{(.*,)?(Alias\(([\x2a-\x7e]+)\))(,.*)?}$");
            Regex rgxMandatory = new Regex(@"(?i)^{(.*,)?(Mandatory)(,.*)?}$");
            Regex rgxNegatable = new Regex(@"(?i)^{(.*,)?(Negatable)(,.*)?}$");
            Regex rgxNotNullOrEmpty = new Regex(@"(?i)^{(.*,)?(NotNullOrEmpty)(,.*)?}$");
            Regex rgxSecret = new Regex(@"(?i)^{(.*,)?(Secret)(,.*)?}$");
            Regex rgxValidateCount = new Regex(@"(?i)^{(.*,)?(ValidateCount\(((-?\d+)?(,-?\d+)?)\))(,.*)?}$");
            Regex rgxValidateLength = new Regex(@"(?i)^{(.*,)?(ValidateLength\(((-?\d+)?(,-?\d+)?)\))(,.*)?}$");
            Regex rgxValidatePattern = new Regex(@"(?i)^{(.*,)?(ValidatePattern\(\/(.*)\/\))(,.*)?}$");
            Regex rgxValidateRange = new Regex(@"(?i)^{(.*,)?(ValidateRange\(((-?\d+)?(,-?\d+)?)\))(,.*)?}$");
            Regex rgxValidateSetOr = new Regex(@"(?i)^{(.*,)?(ValidateSet\(((\w+)(\|\w+)+)\))(,.*)?}$");
            Regex rgxValidateSetAnd = new Regex(@"(?i)^{(.*,)?(ValidateSet\(((\w+)(&\w+)+)\))(,.*)?}$");
            // ================================================================================================================================================
            //   If requested show that we have started
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            WriteDebugLevel1("Preprocessing definition: " + Definition);
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   Primary validation (rgxDefinition). If this one fails we might as well quit.....
            // --------------------------------------------------------------------------------------------------------------------------------------------
            Match cliMatch = rgxDefinition.Match(Definition);
            if (!cliMatch.Success) { WriteError("Definition rule \"" + Definition + "\" not recognized, please validate."); }
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   Start a new cliDataset and initialize its values
            // --------------------------------------------------------------------------------------------------------------------------------------------
            cliDataSet cli = new cliDataSet();
            cli.isPresent = cli.hasArgument = cli.isMandatory = cli.isSecret = cli.isNegated = cli.isNegatable = false;
            cli.notNullOrEmpty = cli.doValidateCount = cli.doValidateLength = cli.doValidatePattern = cli.doValidateRange = cli.doValidateSet = false;
            cli.fullSwitchName = cli.fullAliasName = cli.type = cli.argumentValue = cli.defaultValue = cli.validatePattern = cli.validateSet = "";
            cli.validateCount = cli.validateLength = cli.validateRange = new string[2];
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   Process the passed definition and store the info in the dataset
            // --------------------------------------------------------------------------------------------------------------------------------------------
            string validationRules = (cliMatch.Groups[1].Value != "") ? cliMatch.Groups[1].Value : "";
            cli.type = (cliMatch.Groups[2].Value != "") ? cliMatch.Groups[2].Value.ToLower() : "[string]";
            if (!typeList.Contains(cli.type.ToLower())) { WriteError(cli.fullSwitchName + " - Invalid type definition"); }
            cli.fullSwitchName = cliMatch.Groups[4].Value;
            cli.argumentValue = cli.defaultValue = (cliMatch.Groups[5].Value != "") ? cliMatch.Groups[5].Value.TrimStart('=') : "";
            if (cli.argumentValue.Length > 0) { cli.hasArgument = true; }
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   Get the switch validation string, pre-process it if it isn't empty. Cannot use a Regex: rules can contain any possible character!
            // --------------------------------------------------------------------------------------------------------------------------------------------
            if (validationRules != null)
            {
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   If set get the switch alias, store it in its own dictionary and remove it from validaton string.
                // ----------------------------------------------------------------------------------------------------------------------------------------
                Match valMatch = rgxAlias.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.fullAliasName = valMatch.Groups[3].Value;
                    aliasDictionary.Add(cli.fullAliasName, cli.fullSwitchName);
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   If Mandatory is set, set the flag and remove it from validaton string and add it to to the localArgumentList
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxMandatory.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.isMandatory = true; cli.isPresent = true;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   If negatable is set, set the flag and remove it from validaton string.
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxNegatable.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.isNegatable = true;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   If mandatory is set, set the flag and remove it from validaton string.
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxNotNullOrEmpty.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.notNullOrEmpty = true;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   If secret is set, set the flag and remove it from validaton string. Also sets the notnullorempty flag!
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxSecret.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.isSecret = true; cli.notNullOrEmpty = true;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   ValidateCount(x) or ValidateCount(x,y) -> x-only then y=0, y-only than x=1
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxValidateCount.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.doValidateCount = true;
                    string rangeValues = valMatch.Groups[3].Value;
                    string[] mmValues = rangeValues.Split(',');
                    if (mmValues.Length > 2) { WriteError(cli.fullSwitchName + " - To many arguments for ValidateCount (" + rangeValues + ") - specification should be (min,max)"); }
                    if (mmValues.Length == 1) { mmValues = new string[] { mmValues[0], "0" }; }
                    if (mmValues[0].Length == 0) { mmValues[0] = "1"; }
                    cli.validateCount = mmValues;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   ValidateLength(x) or ValidateLength(x,y) -> x-only then y=0, y-only than x=1
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxValidateLength.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.doValidateLength = true;
                    string lengthValues = valMatch.Groups[3].Value;
                    string[] mmValues = lengthValues.Split(',');
                    if (mmValues.Length > 2) { WriteError(cli.fullSwitchName + " - To many arguments for ValidateLength (" + lengthValues + ") - specification should be (min,max)"); }
                    if (mmValues.Length == 1) { mmValues = new string[] { mmValues[0], "0" }; }
                    if (mmValues[0].Length == 0) { mmValues[0] = "1"; }
                    cli.validateLength = mmValues;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   ValidatePattern - Regex must be encapsulated by (/ and /)
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxValidatePattern.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.doValidatePattern = true;
                    cli.validatePattern = valMatch.Groups[3].Value;
                    try { new Regex(cli.validatePattern); }
                    catch { WriteError(cli.fullSwitchName + " - ValidatePattern - regex pattern does not compile."); }
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   ValidateRange(x,y) -> value must be between x & y. Perform type based checking!
                //   Value type can be one of byte, sbyte, decimal, float, double, int, uint, long, ulong, short, ushort (strip "[]" if present!)
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxValidateRange.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.doValidateRange = true;
                    object minValue = null; bool minFail = false;
                    object maxValue = null; bool maxFail = false;
                    string rangeValues = valMatch.Groups[3].Value;
                    object[] mmValues = rangeValues.Split(',');
                    if (mmValues.Length > 2) { WriteError(cli.fullSwitchName + " - To many arguments for ValidateRange (" + rangeValues + ") - specification should be (min,max)"); }
                    switch (cli.type.Replace("[]", ""))
                    {
                        case "[byte]": minValue = Byte.MinValue; maxValue = Byte.MaxValue;
                            try { minFail = (Convert.ToByte(mmValues[0]) < Convert.ToByte(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToByte(mmValues[1]) > Convert.ToByte(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[sbyte]": minValue = SByte.MinValue; maxValue = SByte.MaxValue;
                            try { minFail = (Convert.ToSByte(mmValues[0]) < Convert.ToSByte(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToSByte(mmValues[1]) > Convert.ToSByte(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[decimal]": minValue = Decimal.MinValue; maxValue = Decimal.MaxValue;
                            try { minFail = (Convert.ToDecimal(mmValues[0]) < Convert.ToDecimal(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToDecimal(mmValues[1]) > Convert.ToDecimal(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[float]": minValue = Single.MinValue; maxValue = Single.MaxValue;
                            try { minFail = (Convert.ToSingle(mmValues[0]) < Convert.ToSingle(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToSingle(mmValues[1]) > Convert.ToSingle(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[double]": minValue = Double.MinValue; maxValue = Double.MaxValue;
                            try { minFail = (Convert.ToDouble(mmValues[0]) < Convert.ToDouble(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToDouble(mmValues[1]) > Convert.ToDouble(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[short]": minValue = Int16.MinValue; maxValue = Int16.MaxValue;
                            try { minFail = (Convert.ToInt16(mmValues[0]) < Convert.ToInt16(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToInt16(mmValues[1]) > Convert.ToInt16(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[ushort]": minValue = UInt16.MinValue; maxValue = UInt16.MaxValue;
                            try { minFail = (Convert.ToUInt16(mmValues[0]) < Convert.ToUInt16(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToUInt16(mmValues[1]) > Convert.ToUInt16(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[int]": minValue = Int32.MinValue; maxValue = Int32.MaxValue;
                            try { minFail = (Convert.ToInt32(mmValues[0]) < Convert.ToInt32(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToInt32(mmValues[1]) > Convert.ToInt32(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[unint]": minValue = UInt32.MinValue; maxValue = UInt32.MaxValue;
                            try { minFail = (Convert.ToUInt32(mmValues[0]) < Convert.ToUInt32(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToUInt32(mmValues[1]) > Convert.ToUInt32(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[long]": minValue = Int64.MinValue; maxValue = Int64.MaxValue;
                            try { minFail = (Convert.ToInt64(mmValues[0]) < Convert.ToInt64(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToInt64(mmValues[1]) > Convert.ToInt64(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        case "[ulong]": minValue = UInt64.MinValue; maxValue = UInt64.MaxValue;
                            try { minFail = (Convert.ToUInt64(mmValues[0]) < Convert.ToUInt64(minValue)); }
                            catch { minFail = true; }
                            try { maxFail = (Convert.ToUInt64(mmValues[1]) > Convert.ToUInt64(maxValue)); }
                            catch { maxFail = true; }
                            break;

                        default: break;
                    }
                    if (mmValues.Length == 1) { mmValues = new object[] { mmValues[0], maxValue.ToString() }; }
                    if (Convert.ToString(mmValues[0]).Length == 0) { mmValues[0] = minValue.ToString(); }
                    if (minFail) { WriteError(cli.fullSwitchName + " - ValidateRange min " + cli.type + " value (" + mmValues[0] + ",y) is out of type range. Min: " + minValue); }
                    if (maxFail) { WriteError(cli.fullSwitchName + " - ValidateRange max " + cli.type + " value (x," + mmValues[1] + ") is out of type range. Max: " + maxValue); }
                    cli.validateRange = new string[] { (string)mmValues[0], (string)mmValues[1] };
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   ValidateSet(x&y&...) or ValidateSet(x|y|...) - Keep this the 1st in the execution order, the set might contain rule directives!
                // ----------------------------------------------------------------------------------------------------------------------------------------
                valMatch = rgxValidateSetOr.Match(validationRules);
                if (valMatch.Success)
                {
                    cli.doValidateSet = true;
                    cli.validateSet = valMatch.Groups[3].Value;
                    validationRules = validationRules.Remove(valMatch.Groups[2].Index, valMatch.Groups[2].Length);
                }
                else { valMatch = rgxValidateSetAnd.Match(validationRules); }
                // ----------------------------------------------------------------------------------------------------------------------------------------
                //   All possibilities of the validation entries have been verified, and the original validationRules should be empty (well almost)
                //   Remove 'balast' and verify, if not an invalid rule has been entered (could be a typo). Inform the developer and quit
                // ----------------------------------------------------------------------------------------------------------------------------------------
                while (validationRules.Contains(",,")) { validationRules = validationRules.Replace(",,", ","); }
                validationRules = validationRules.Trim(cBrackets).TrimEnd(',');
                if (validationRules.Length > 1) { WriteError(cli.fullSwitchName + " - Invalid validation rule {..." + validationRules + "...}"); }
                // ----------------------------------------------------------------------------------------------------------------------------------------
            }
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   Save the cli data results in the switch dictionary.
            //   If wanted show the cli data details and return to caller
            // --------------------------------------------------------------------------------------------------------------------------------------------
            switchDictionary.Add(cli.fullSwitchName, cli);
            //ShowCLIdetails(cli);
            return;
        }

        // ====================================================================================================================================================
        //   cli.AddRule adds an Allow, Disallow (or any other TDB rule) to the checkRules list. This list is used to check for invalid switch combinations
        // ====================================================================================================================================================
        public static void AddRule(string Rule)
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Use a Regex expression to verify the rule.
            //   - Disallow record:  "^\<(disallow any2)\((.*)\)\>$"
            //   - Allow record:     "^\<(allow any)\((.*)\)\>$"
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            rgxAllowAny = new Regex(@"^\<(allow any)\((.*)\)\>$");
            rgxDisallow = new Regex(@"^\<(disallow any2)\((.*)\)\>$");
            // ================================================================================================================================================
            //   If requested show that we have started
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            WriteDebugLevel1("Preprocessing rule: " + Rule);
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   1st validation (rgxAllowAny). If it is a AllowAny rule, store it and go for the next rule/definition
            // --------------------------------------------------------------------------------------------------------------------------------------------
            Match cliMatch = rgxAllowAny.Match(Rule);
            if (cliMatch.Success) { checkRules.Add(Rule); return; }
            // --------------------------------------------------------------------------------------------------------------------------------------------
            //   2nd validation (rgxDiaAllow). If it is a DisAllow rule, store it and go for the next rule/definition
            // --------------------------------------------------------------------------------------------------------------------------------------------
            cliMatch = rgxDisallow.Match(Rule);
            if (cliMatch.Success) { checkRules.Add(Rule); return; }
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   A match should have occured by now. If not issue a warning and quit
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            WriteError("Invalid rule definition " + Rule + " Terminating action now.");
            return;
        }

        // ====================================================================================================================================================
        //   cli.IsPresent takes the passed switch name and will return true or false depending on whether the switch is present on the command line.
        //   Mandatory switchs witll of cause always return true ;)
        // ====================================================================================================================================================
        public static bool IsPresent(string Switch)
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Try to load the dataset of the given switch. If not available return false, else return the value of isPresent
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            cliDataSet cliData = new cliDataSet();
            try { cliData = switchDictionary[Switch]; }
            catch { WriteError(Switch + " is not a valid switchname, cli.IsPreset is terminating."); }
            return cliData.isPresent;
        }

        // ====================================================================================================================================================
        //   cli.IsNegated takes the passed switch name and will return true or false depending on whether the switch is specifically negated or not.
        // ====================================================================================================================================================
        public static bool IsNegated(string Switch)
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Try to load the dataset of the given switch. If not available return false, else return the value of isPresent
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            cliDataSet cliData = new cliDataSet();
            try { cliData = switchDictionary[Switch]; }
            catch { WriteError(Switch = " is not a valid switchname, cli.IsNegated is terminating."); }
            return cliData.isNegated;
        }

        // ====================================================================================================================================================
        //   cli.GetValue returns the argument value for the given switch
        // ====================================================================================================================================================
        public static string GetValue(string Switch)
        {
            // ================================================================================================================================================
            //  Before doing anything else... Check for the TotalCLI specific argument cliDebugLevelX
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Try to load the dataset of the given switch. If not available return false, else return the value of isPresent
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            cliDataSet cliData = new cliDataSet();
            try { cliData = switchDictionary[Switch]; }
            catch { WriteError(Switch = " is not a valid switchname, cli.GetValue is terminating."); }
            return cliData.argumentValue;
        }

        // ====================================================================================================================================================
        //   cli.LoadDefinitions will use the added definitions to process the command-line arguments.
        // ====================================================================================================================================================
        public static void LoadDefinitions()
        {
            cliDataSet cliData = new cliDataSet();
            cliDefinitionsLoaded = false;
            // ================================================================================================================================================
            //  Before doing anything else: check the debug state and whether a switch identifier has been set
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            CheckDebugState();
            if (cliSwitchIdentifier  == null) { SetSwitchIdentifier(); }
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Convert the commandline argument string to a list and remove the 1st elelement (image name) and the cliDebug arguments
            //   Also make sure a switch identifier has been set
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            List<string> argumentList = new List<string>();
            string argValueString = "";
            string[] cmndLineArgs = Environment.GetCommandLineArgs();
            WriteDebugLevel3("Original argumentlist: [" + string.Join("] [", cmndLineArgs) + "]");
            for (int i = 1; i < cmndLineArgs.Length; i++)
            {
                if (cmndLineArgs[i].StartsWith("--cliDebugLevel")) { continue; }
                if (!cmndLineArgs[i].StartsWith(cliSwitchIdentifier)) { argValueString += cmndLineArgs[i] + ","; continue; }
                if (argValueString.Length > 0) { argumentList.Add(argValueString.TrimEnd(',')); argValueString = ""; }
                argumentList.Add(cmndLineArgs[i]);
            }
            // Do not forget to save the last set of argument values (if any!)
            if (argValueString.Length > 0) { argumentList.Add(argValueString.TrimEnd(',')); argValueString = ""; }
            string workingArgumentList = ""; foreach (string arg in argumentList) { workingArgumentList += "[" + arg + "] "; }
            WriteDebugLevel3("Working argumentlist: " + workingArgumentList);
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Now process the remaining arguments. When starting with the switch identifier, get the fullSwitchName and fetch its dataset 
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            bool switchFound = false;
            string switchName = "", aliasName = "";
            foreach (string thisArgument in argumentList)
            {
                if (!thisArgument.StartsWith(cliSwitchIdentifier))
                {
                    if (!switchFound) { WriteError("Unassigned argument " + thisArgument + " found."); }
                    cliData.argumentValue = thisArgument;
                    cliData.hasArgument = true;
                    switchDictionary[switchName] = cliData;
                    switchFound = false;
                }
                else
                {
                    bool switchIsNegated = false, switchIsAlias = false;
                    string argValue = "";
                    int stdAliasCnt = 0, negAliasCnt = 0, stdSwitchCnt = 0, negSwitchCnt = 0;
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   First check whether the argument is a alias or a negated alias. A switch starting with "no" is not perse negated (aka /nonsense).
                    //   So if stdSwitchCnt or stdAliasCnt is > 0 skip the negated check
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    switchName = thisArgument.Substring(cliSwitchIdentifier.Length);
                    if (switchName.Contains('=')) { argValue = switchName.Split('=')[1]; switchName = switchName.Split('=')[0]; }
                    if (switchName.Contains(':')) { argValue = switchName.Split(':')[1]; switchName = switchName.Split(':')[0]; }
                    stdSwitchCnt = switchDictionary.Count(d => d.Key.StartsWith(switchName, StringComparison.OrdinalIgnoreCase));
                    stdAliasCnt = aliasDictionary.Count(d => d.Key.StartsWith(switchName, StringComparison.OrdinalIgnoreCase));
                    if ((stdSwitchCnt == 0) & (stdAliasCnt == 0) & switchName.StartsWith("no"))
                    {
                        switchIsNegated = true;
                        negAliasCnt  = aliasDictionary.Count(d => d.Key.StartsWith(switchName.Substring(2)));
                        negSwitchCnt = switchDictionary.Count(d => d.Key.StartsWith(switchName.Substring(2)));
                    }
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   There can be only 1 !!! So if the total count is < 1 or > 1 we have a problem. Say so and quit
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    int switchCount = stdAliasCnt + negAliasCnt + stdSwitchCnt + negSwitchCnt;
                    if (switchCount > 1) { WriteError("Switch " + thisArgument + " is ambiguous."); }
                    if (switchCount < 1) { WriteError("Switch " + thisArgument + " is unknown."); }
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   If negAliasCnt or negSwitchCnt = 1 the switch is negated! Set the flag and trim the switch.
                    //   If stdAliasCnt or negAliasCnt = 1 the switch is an alias! Set the flag and save switch as alias
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    if (negAliasCnt + negSwitchCnt == 1) { switchIsNegated = true; switchName = switchName.Substring(2); }
                    if (stdAliasCnt + negAliasCnt == 1) { switchIsAlias = true; aliasName = switchName; }
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   Time to get the full switch name and load its cliDataSet.
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    if (switchIsAlias)
                    {
                        var keyPair = aliasDictionary.Where(d => d.Key.StartsWith(aliasName, StringComparison.OrdinalIgnoreCase));
                        foreach (var pair in keyPair) { switchName = pair.Value; }
                    }
                    else
                    {
                        var keyPair = switchDictionary.Where(d => d.Key.StartsWith(switchName, StringComparison.OrdinalIgnoreCase));
                        foreach (var pair in keyPair) { switchName = pair.Key; }
                    }
                    if (!switchIsAlias) { WriteDebugLevel1("Processing switch: " + switchName); }
                    else { WriteDebugLevel1("Processing switch: " + switchName + " (from Alias: " + aliasName + ")"); }
                    cliData = switchDictionary[switchName];
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   Add the full switchname to a comma-separated list which can be used to match is with an aloow/disallow rule.
                    //   Update the dataset with what we found sofar. Also check whether a negated switch/alias is allowed to be negated!
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    cliData.isPresent = true;
                    cliData.isNegated = switchIsNegated;
                    if (!cliData.isNegatable && cliData.isNegated) { WriteError("Switch " + thisArgument + " is not negatable"); }
                    if (cliData.type == "[bool]")   { cliData.hasArgument = true; if (switchIsNegated) { cliData.argumentValue = "False"; } else { cliData.argumentValue = "True"; } }
                    if (argValue.Length > 0) { cliData.argumentValue = argValue; cliData.hasArgument = true; }
                    switchDictionary[switchName] = cliData;
                    switchFound = true;
                }
            }
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Now scan the switchDictionary for all switches with the isPresent flag set, and collect these in a separate switchList
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            foreach (var pair in switchDictionary)
            {
                if (pair.Value.isPresent)
                {
                    switchList.Add(pair.Key);
                    WriteDebugLevel1(String.Format("Switch List: {0,-20} - Is present: {1}", pair.Key, pair.Value.isPresent));
                    ShowCLIdetails(pair.Value);
                }
            }
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            //   Match the found commandline switches with a possible allow/disallow rule
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            if ((checkRules.Count > 0) & (switchList.Count > 0))
            {
                foreach (string chkRule in checkRules)
                {
                    int disallowCnt = 0; int allowanyCnt = 0;
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   Is it a DisAllow check??
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    Match ruleCheck = rgxDisallow.Match(chkRule);
                    if (ruleCheck.Success)
                    {
                        string disallowRule = ruleCheck.Groups[2].Value;
                        foreach (string token in disallowRule.Split(',')) { if (switchList.Contains(token)) { disallowCnt += 1; } }
                    }
                    if (disallowCnt > 1) { WriteError("Switch conflict due to \"" + chkRule + "\""); }
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    //   Is it a AllowAny check?? (still need to figure out what it can be used for - aka when to warn)
                    // ----------------------------------------------------------------------------------------------------------------------------------------
                    ruleCheck = rgxAllowAny.Match(chkRule);
                    if (ruleCheck.Success)
                    {
                        string allowanyRule = ruleCheck.Groups[2].Value;
                        foreach (string token in allowanyRule.Split(',')) { if (switchList.Contains(token)) { allowanyCnt += 1; } }
                    }
                    //if (disallowCnt > 1) { WriteError("Switch conflict due to \"" + chkRule + "\""); }
                }
            }
            // ================================================================================================================================================
            //   Peform the validation checks
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            foreach (string keyName in switchList)
            {
                cliData = switchDictionary[keyName];
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   Switch is mandatory but does not have a value? Get it from the command line (interactive mode only!)
                //   When prompted for input and nothing is entered: Quit!
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.isMandatory && !cliData.hasArgument)
                {
                    string errMsg = "Value for mandatory switch " + keyName + " is missing, termination request.";
                    if (!Environment.UserInteractive) { WriteError(errMsg); }
                    Console.Write("Please enter mandatory " + keyName + ": "); cliData.argumentValue = Console.ReadLine();
                    if ((cliData.argumentValue == null) || (cliData.argumentValue.Length < 1)) { WriteError(errMsg); }
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   Parameter is NotNullOrEmpty but does not have a value? Get it from the command line (only interactive mode!)
                //   When prompted for input and nothing is entered: Quit!
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.notNullOrEmpty && !cliData.hasArgument)
                {
                    string errMsg = "Value for switch " + keyName + " is missing, termination request.";
                    if (!Environment.UserInteractive) { WriteError(errMsg); }
                    string keyword = (cliData.isSecret) ? "Secret" : "NotNulOrEmpty";
                    Console.Write("Please enter " + keyword + " " + keyName + ": ");
                    if (cliData.isSecret) { cliData.argumentValue = ReadPassword(); }
                    else { cliData.argumentValue = Console.ReadLine(); }
                    if ((cliData.argumentValue == null) || (cliData.argumentValue.Length < 1)) { WriteError(errMsg); }
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   validate Count: cliData.argumentValue must have at least (x) elements and maximal (y) elements
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.doValidateCount)
                {
                    int countLow  = Convert.ToInt32(cliData.validateCount[0]);
                    int countHigh = Convert.ToInt32(cliData.validateCount[1]);
                    int elemCount = cliData.argumentValue.Split(',').Length; if (countHigh == 0) { countHigh = elemCount; }
                    if (elemCount < countLow) { WriteError("Minimal " + countLow + " values required for switch " + keyName); }
                    if (elemCount > countHigh) { WriteError("Maximal " + countHigh + " values allowed for switch " + keyName); }
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   validate Length: minimum (x) and/or maximum (y) length of cliData.argumentValue
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.doValidateLength)
                {
                    int lengthLow  = Convert.ToInt32(cliData.validateLength[0]);
                    int lengthHigh = Convert.ToInt32(cliData.validateLength[1]);
                    foreach (string argValue in cliData.argumentValue.Split(','))
                    {
                        int elemLength = argValue.Length; if (lengthHigh == 0) { lengthHigh = elemLength; }
                        if (elemLength < lengthLow) { WriteError(keyName + " - Minimal required value length for switch value " + argValue + " is " + lengthLow); }
                        if (elemLength > lengthHigh) { WriteError(keyName + " - Maximal allowed value length for switch value " + argValue + " is " + lengthHigh); }
                    }
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   validate Range: cliData.argumentValue value must be > (x) and/or < (y) Check is valueType dependend!
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.doValidateRange)
                {
                    foreach (string argValue in cliData.argumentValue.Split(','))
                    {
                        bool minFail = false; bool maxFail = false;
                        switch (cliData.type.Replace("[]", ""))
                        {
                            case "[byte]":    minFail = (Convert.ToByte(argValue) < Convert.ToByte(cliData.validateRange[0]));
                                              maxFail = (Convert.ToByte(argValue) > Convert.ToByte(cliData.validateRange[1]));
                                              break;

                            case "[sbyte]":   minFail = (Convert.ToSByte(argValue) < Convert.ToSByte(cliData.validateRange[0]));
                                              maxFail = (Convert.ToSByte(argValue) > Convert.ToSByte(cliData.validateRange[1]));
                                              break;

                            case "[decimal]": minFail = (Convert.ToDecimal(argValue) < Convert.ToDecimal(cliData.validateRange[0]));
                                              maxFail = (Convert.ToDecimal(argValue) > Convert.ToDecimal(cliData.validateRange[1]));
                                              break;

                            case "[float]":   minFail = (Convert.ToSingle(argValue) < Convert.ToSingle(cliData.validateRange[0]));
                                              maxFail = (Convert.ToSingle(argValue) > Convert.ToSingle(cliData.validateRange[1]));
                                              break;

                            case "[double]":  minFail = (Convert.ToDouble(argValue) < Convert.ToDouble(cliData.validateRange[0]));
                                              maxFail = (Convert.ToDouble(argValue) > Convert.ToDouble(cliData.validateRange[1]));
                                              break;

                            case "[short]":   minFail = (Convert.ToInt16(argValue) < Convert.ToInt16(cliData.validateRange[0]));
                                              maxFail = (Convert.ToInt16(argValue) > Convert.ToInt16(cliData.validateRange[1]));
                                              break;

                            case "[ushort]":  minFail = (Convert.ToUInt16(argValue) < Convert.ToUInt16(cliData.validateRange[0]));
                                              maxFail = (Convert.ToUInt16(argValue) > Convert.ToUInt16(cliData.validateRange[1]));
                                              break;

                            case "[int]":     minFail = (Convert.ToInt32(argValue) < Convert.ToInt32(cliData.validateRange[0]));
                                              maxFail = (Convert.ToInt32(argValue) > Convert.ToInt32(cliData.validateRange[1]));
                                              break;

                            case "[unint]":   minFail = (Convert.ToUInt32(argValue) < Convert.ToUInt32(cliData.validateRange[0]));
                                              maxFail = (Convert.ToUInt32(argValue) > Convert.ToUInt32(cliData.validateRange[1]));
                                              break;

                            case "[long]":    minFail = (Convert.ToInt64(argValue) < Convert.ToInt64(cliData.validateRange[0]));
                                              maxFail = (Convert.ToInt64(argValue) > Convert.ToInt64(cliData.validateRange[1]));
                                              break;

                            case "[ulong]":   minFail = (Convert.ToUInt64(argValue) < Convert.ToUInt64(cliData.validateRange[0]));
                                              maxFail = (Convert.ToUInt64(argValue) > Convert.ToUInt64(cliData.validateRange[1]));
                                              break;

                            default:          break;
                        }
                        if (minFail) { WriteError(keyName + " argument value " + argValue + " is not in range, min value is " + cliData.validateRange[0]); }
                        if (maxFail) { WriteError(keyName + " argument value " + argValue + " is not in range, max value is " + cliData.validateRange[1]); }
                    }
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   validate Set: cliData.validateSet contains "|" -> cliData.argumentValue can only be ONE of the set values
                //                 cliData.validateSet contains "&" -> cliData.argumentValue can only be one or more of the set values
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.doValidateSet)
                {
                    bool orCheck = cliData.validateSet.Contains('|');
                    bool andCheck = cliData.validateSet.Contains('&');
                    string newArgumentValue = "";
                    if (orCheck & andCheck) { WriteError("Invalid combination of AND and OR directives in validationSet " + cliData.validateSet); }
                    string[] checkSet = cliData.validateSet.Replace('&', '|').Split('|');
                    string[] checkValues = cliData.argumentValue.ToLower().Split(',');
                    int valCount = checkValues.Length; int matchCount = 0;
                    if ((orCheck) && (valCount > 1)) { WriteError("Too many values specified for switch " + keyName + ", validation set only allows for 1 value."); }
                    foreach (string chkVal in checkValues)
                    {
                        matchCount = 0;
                        foreach (string chkElm in checkSet)
                        {
                            if (chkElm.ToLower().StartsWith(chkVal.ToLower())) { newArgumentValue += chkElm + ","; matchCount += 1; }
                        }
                    }
                    if ((matchCount == 0) || (orCheck & (matchCount > 1)))
                    { WriteError("Invalid value " + cliData.argumentValue + " for switch " + keyName + " Select value from: " + cliData.validateSet); }
                    cliData.argumentValue = newArgumentValue.TrimEnd(',');
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   validate Pattern: Regex check cliData.argumentValue against the specified pattern 
                // --------------------------------------------------------------------------------------------------------------------------------------------
                if (cliData.doValidatePattern)
                {
                    Regex valPattern = new Regex(cliData.validatePattern);
                    Match patCheck = valPattern.Match(cliData.argumentValue);
                    if (!patCheck.Success) { WriteError("Invalid value " + cliData.argumentValue + " for switch " + keyName + "\n\tPattern mismatch: " + cliData.validatePattern); }
                }
                // --------------------------------------------------------------------------------------------------------------------------------------------
                //   All checks passed? Then save all data in the dictionary.
                // --------------------------------------------------------------------------------------------------------------------------------------------
                switchDictionary[keyName] = cliData;
            }
            // ================================================================================================================================================
            //   Finally - Raise the cliDefinitionsLoaded flag and return
            // ================================================================================================================================================
            cliDefinitionsLoaded = true;
        }
    
    }
    // --------------------------------------------------------------------------------------------------------------------------------------------------------
    // End of class cli
    // --------------------------------------------------------------------------------------------------------------------------------------------------------
}
// ============================================================================================================================================================
//    EOF, Sayonara!
// ============================================================================================================================================================