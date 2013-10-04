TO USE THIS PACKAGE

1. Download and install Visual C# Express: 
http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express
2. Install NUnit (http://nunit.org)
3. Build the TradeLinkSuite.sln solution.
4. Use BuildInstaller.bat to create an installer

FOR MORE INFORMATION : http://code.google.com/p/tradelink

To build the BrokerServer c++ broker connectors :
1. Install visual studio 2005 professional
2. Build brokerservers\brokerserver.sln file in release mode
3. Add empty file install\_includebs.txt to trunk path
4. Run BuildInstaller.bat to create install with brokerservers included
