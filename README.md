# TotalCLI
###Validation and Processing of Command Line Input

While in search of an easy method for processing/validating command line arguments I found a lot of hints, examples and instructions on the internet. The examples and submissions I found though did each provide about 80% of the functionality I needed, and the lack of the 20% made me decide to write the TotalCLI (cli.*) functions. 

#### The cli Functions
There are three types of functions;
#####Definition functions
- cli.AddDefinition
- cli.AddRule

The functions are used to build the switch definiton context. When adding a definition or rule, the added item will be checked for syntax and validity.

- cli.SetSwitchIdentifier
- cli.SetSecretMarker
 
By default the "/" character is used to identify switches (aka: aaa.exe /debug). Use cli.SetSwitchIdentifier(...) to define your own (aka: cli.SetSwitchIdentifier("^") will result in aaa.exe ^debug Be careful with what you choose, the OS might play tricks on you!

When prompted for and entering a secret value, the entered characters are masked with a ‘marker’.
By default TotalCLI uses the ‘#’ character, but use cli.SetSecretMarker(‘..’) to pick a character of your own.

##### Load & Freeze function

- cli.LoadDefinitions

This function will load and freeze the definition context, this includes processing the command line arguments and verify/match them against the provided definitions.

#####Retrieval functions
```
- cli.IsPresent(switchName)	           verify whether a switch has been used
- cli.IsNegated(switchName)	           verify whether a switch has been negated (aka: aaa.exe /nodebug)
- cli.GetValue(switchName)	           retrieve the value assigned to the switch
```

######Usage (C# example):
* Define a set of definitions.
```
	cli.AddDefinition("{notnullorempty}[string]logFile");
	cli.AddDefinition("{mandatory}[string]inputFile=.\MyExample.xml");
	cli.AddDefinition("{notnullorempty,validateset(Whether|Version|Status)}[string]show");
	cli.AddDefinition("{negatable,alias(verbose)}[bool]debug");
	cli.AddRule("<disallow any2(logfile,inputfile)");
	cli.SetSwitchIdentifier("--");
```
* Load and freeze the definition context.
```
  cli.LoadDefinitions()
```
* Use the results in the rest of your program
```
  if (cli.IsPresent("debug")) { string dbgValue = cli.GetValue("debug"); }
```
Please read the TotalCLI.rtf for further details
