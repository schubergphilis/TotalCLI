# TotalCLI
###Validation and Processing of Command Line Input

#### Modification history

##### 1.2.0
The handeling of array argument values ([string[]], [int[]], etc.) had a bug resulting in only passing the 1st value of a list.
This has been corrected and an array of values is now passed as a single element of comma separated values
The internal TotalCLI debug mode was not always showing everything it should. The following levels are available and supplement each other
- --cliDebugLevel1 lowest level, least information shown
- --cliDebugLevel2 medium level, shows level1 info + some extras
- --cliDebugLevel3 highest level, shows level1 + level2 info + some extras
