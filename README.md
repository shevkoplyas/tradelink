tradelink
=========

from:	 Dmitry Shevkoplyas
to:	 tradelink-free@googlegroups.com
date:	 Fri, Oct 4, 2013 at 12:00 AM
subject:	 Re: Source Code

hi all,

Latest TradeLink source code wanted.
I have revision 3754 which was committed on 07-Aug-2013 and I checked out it next day 08-Aug-2013. 
Does anyone has more recent TradeLink sources? 

On 18-Sep-2013 whole svn repo was "flattened" (no more history available) and commiter "Joshua Franta" <josh@pracplay.com> put following comments: "removing sensitive passwords/sensitive info", but PracPlay also deleted ASP, Kadina, Quotopia. They basically crippled the whole idea of TradeLink without anouncing thiers plans! They could do this in some more community friendly way, but was done is done.

This latest revision 3754 still has ASP, Kadina, Quotopia etc. It also has "trunc" along with some development brunches:

branches
+---FixTheBuild
+---gregj-branch
+---issue229
L---TD-ActiveX
tags
trunk
wiki

full tree of my latest rev3754 TradeLink source looks like this:

src-pure
+---branches
¦   +---FixTheBuild
¦   ¦   +---ASP
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---BrokerServers
¦   ¦   ¦   +---AnvilServer
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   +---debug
¦   ¦   ¦   +---Install
¦   ¦   ¦   +---LightspeedServer
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   +---include
¦   ¦   ¦   ¦   ¦   L---LightspeedTrader
¦   ¦   ¦   ¦   +---lib
¦   ¦   ¦   ¦   +---Release
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   +---release
¦   ¦   ¦   +---ServerGenesis
¦   ¦   ¦   ¦   +---INC
¦   ¦   ¦   ¦   +---INC32
¦   ¦   ¦   ¦   L---Res
¦   ¦   ¦   +---TestTradeLibFast
¦   ¦   ¦   ¦   +---cfix
¦   ¦   ¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   ¦   +---include
¦   ¦   ¦   ¦   ¦   L---lib
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   +---TradeLibFast
¦   ¦   ¦   ¦   +---Release
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   L---TWSServer
¦   ¦   ¦       +---Release
¦   ¦   ¦       +---res
¦   ¦   ¦       L---Shared
¦   ¦   +---Chartographer
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---CQG2EPF
¦   ¦   ¦   L---Properties
¦   ¦   +---Gauntlet
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Install
¦   ¦   ¦   L---talib_excel
¦   ¦   +---Kadina
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Quotopia
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   L---Debug
¦   ¦   ¦   ¦       L---Properties
¦   ¦   ¦   +---Properties
¦   ¦   ¦   L---Resources
¦   ¦   +---Record
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Replay
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Responses
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerBlackwood
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerDAS
¦   ¦   ¦   +---dll
¦   ¦   ¦   +---Properties
¦   ¦   ¦   L---Resources
¦   ¦   +---ServerDBFX
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   L---Debug
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerEsignal
¦   ¦   ¦   +---Properties
¦   ¦   ¦   L---Resources
¦   ¦   +---ServerFix
¦   ¦   ¦   +---dll
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerGrayBox
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   L---Debug
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerIQ-DTN
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerMB
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerNxCore
¦   ¦   ¦   +---dll
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerNxCore64
¦   ¦   ¦   +---dll
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerRealTick
¦   ¦   ¦   +---dll
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerRedi
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerRithmic
¦   ¦   ¦   +---dll
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerSterling
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerTD
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerTDX
¦   ¦   ¦   +---ActiveXComp
¦   ¦   ¦   ¦   +---ActiveX
¦   ¦   ¦   ¦   L---DLL
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   L---Debug
¦   ¦   ¦   +---obj
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerZenfire
¦   ¦   ¦   +---dll
¦   ¦   ¦   L---Properties
¦   ¦   +---TikConverter
¦   ¦   ¦   L---Properties
¦   ¦   +---TikData
¦   ¦   +---TimeAndSales
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkAPI
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkAppKit
¦   ¦   ¦   +---Google
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkCommon
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkResearch
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   L---TradeLinkTests
¦   ¦       L---Properties
¦   +---gregj-branch
¦   ¦   +---API
¦   ¦   +---ASP
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   +---obj
¦   ¦   ¦   ¦   L---Debug
¦   ¦   ¦   ¦       L---TempPE
¦   ¦   ¦   L---Properties
¦   ¦   +---BranchV1
¦   ¦   +---BrokerServers
¦   ¦   ¦   +---AnvilServer
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   +---debug
¦   ¦   ¦   +---Install
¦   ¦   ¦   +---release
¦   ¦   ¦   +---ServerGenesis
¦   ¦   ¦   ¦   +---INC
¦   ¦   ¦   ¦   +---INC32
¦   ¦   ¦   ¦   L---Res
¦   ¦   ¦   +---TestTradeLibFast
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   +---TradeLibFast
¦   ¦   ¦   ¦   +---Release
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   L---TWSServer
¦   ¦   ¦       +---Release
¦   ¦   ¦       +---res
¦   ¦   ¦       L---Shared
¦   ¦   +---Chartographer
¦   ¦   +---CQG2EPF
¦   ¦   +---Gauntlet
¦   ¦   +---HistoricalPriceParser
¦   ¦   ¦   L---Properties
¦   ¦   +---InstallSuite
¦   ¦   +---Kadina
¦   ¦   +---LogViewer
¦   ¦   +---Quotopia
¦   ¦   +---Record
¦   ¦   +---Replay
¦   ¦   +---Responses
¦   ¦   +---ResponseTests
¦   ¦   +---ServerBlackwood
¦   ¦   +---ServerDBFX
¦   ¦   +---ServerEsignal
¦   ¦   +---ServerMB
¦   ¦   +---ServerSterling
¦   ¦   +---ServerTD
¦   ¦   +---Tattle
¦   ¦   +---TikConverter
¦   ¦   ¦   L---properties
¦   ¦   +---TikData
¦   ¦   +---TimeAndSales
¦   ¦   +---TradeLinkAPI
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkAppKit
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkCommon
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkResearch
¦   ¦   L---TradeLinkTests
¦   +---issue229
¦   ¦   +---ASP
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---BrokerServers
¦   ¦   ¦   +---AnvilServer
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   +---Install
¦   ¦   ¦   +---TestTradeLibFast
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   +---TradeLibFast
¦   ¦   ¦   ¦   +---Release
¦   ¦   ¦   ¦   L---res
¦   ¦   ¦   L---TWSServer
¦   ¦   ¦       +---Release
¦   ¦   ¦       +---res
¦   ¦   ¦       L---Shared
¦   ¦   +---Chartographer
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---CQG2EPF
¦   ¦   ¦   L---Properties
¦   ¦   +---Gauntlet
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---InstallSuite
¦   ¦   +---Kadina
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Quotopia
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   ¦   L---Properties
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   ¦       L---Properties
¦   ¦   ¦   +---Properties
¦   ¦   ¦   L---Resources
¦   ¦   +---Record
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Replay
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---Responses
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---ResponseTests
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerGenesis
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerMB
¦   ¦   ¦   L---Properties
¦   ¦   +---ServerTD
¦   ¦   ¦   L---Properties
¦   ¦   +---SplitEPF
¦   ¦   ¦   L---Properties
¦   ¦   +---SterServer
¦   ¦   ¦   L---Properties
¦   ¦   +---Tattle
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TestDOM
¦   ¦   ¦   L---TestDOM
¦   ¦   +---TimeAndSales
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkAPI
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkCommon
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkResearch
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TradeLinkTests
¦   ¦   ¦   L---Properties
¦   ¦   +---Tript
¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Release
¦   ¦   ¦   L---Properties
¦   ¦   +---TS2EPF
¦   ¦   ¦   L---Properties
¦   ¦   +---Update
¦   ¦   ¦   L---Properties
¦   ¦   L---Zoom
¦   L---TD-ActiveX
¦       +---ActiveXComp
¦       ¦   +---ActiveX
¦       ¦   L---DLL
¦       +---bin
¦       ¦   L---Debug
¦       +---obj
¦       ¦   L---Debug
¦       ¦       L---TempPE
¦       L---Properties
+---tags
¦   L---REL-0-1-0
¦       +---ASP
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---Box-Examples
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---BrokerServers
¦       ¦   +---AnvilServer
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   +---AnvilServer2
¦       ¦   ¦   +---res
¦       ¦   ¦   L---Shared
¦       ¦   ¦       +---Debug
¦       ¦   ¦       L---Release
¦       ¦   +---Install
¦       ¦   +---TradeLinkServer
¦       ¦   ¦   +---Release
¦       ¦   ¦   L---res
¦       ¦   L---TWSServer
¦       ¦       +---Release
¦       ¦       +---res
¦       ¦       L---Shared
¦       +---Chartographer
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---EPF2IDX
¦       ¦   L---bin
¦       ¦       +---Debug
¦       ¦       L---Release
¦       +---Gauntlet
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---InstallSuite
¦       +---Kadina
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---Quotopia
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   ¦   L---Properties
¦       ¦   ¦   L---Release
¦       ¦   ¦       L---Properties
¦       ¦   L---Properties
¦       +---Record
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---Replay
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---ResearchLib
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---SplitEPF
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---Tattle
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---TimeAndSales
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       +---TradeLib
¦       ¦   +---bin
¦       ¦   ¦   +---Debug
¦       ¦   ¦   L---Release
¦       ¦   L---Properties
¦       L---Tript
¦           +---bin
¦           ¦   +---Debug
¦           ¦   L---Release
¦           L---Properties
+---trunk
¦   +---ASP
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---BrokerServers
¦   ¦   +---AnvilServer
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   +---cfix
¦   ¦   ¦   +---include
¦   ¦   ¦   L---lib
¦   ¦   ¦       +---amd64
¦   ¦   ¦       L---i386
¦   ¦   +---debug
¦   ¦   +---Install
¦   ¦   +---LightspeedServer
¦   ¦   ¦   +---Debug
¦   ¦   ¦   +---include
¦   ¦   ¦   ¦   L---LightspeedTrader
¦   ¦   ¦   +---lib
¦   ¦   ¦   +---Release
¦   ¦   ¦   L---res
¦   ¦   +---release
¦   ¦   +---ServerGenesis
¦   ¦   ¦   +---INC
¦   ¦   ¦   +---INC32
¦   ¦   ¦   L---Res
¦   ¦   +---TestTradeLibFast
¦   ¦   ¦   +---cfix
¦   ¦   ¦   ¦   +---bin
¦   ¦   ¦   ¦   +---include
¦   ¦   ¦   ¦   L---lib
¦   ¦   ¦   L---res
¦   ¦   +---TradeLibFast
¦   ¦   ¦   +---Release
¦   ¦   ¦   L---res
¦   ¦   L---TWSServer
¦   ¦       +---Release
¦   ¦       +---res
¦   ¦       L---Shared
¦   +---Chartographer
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---Gauntlet
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---Install
¦   ¦   L---talib_excel
¦   +---Kadina
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---Quotopia
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   ¦   L---Properties
¦   ¦   ¦   L---Release
¦   ¦   ¦       L---Properties
¦   ¦   +---Properties
¦   ¦   L---Resources
¦   +---Record
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---Replay
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---Responses
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---ServerBlackwood
¦   ¦   +---bin
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---ServerDAS
¦   ¦   +---dll
¦   ¦   +---Properties
¦   ¦   L---Resources
¦   +---ServerDBFX
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---ServerEsignal
¦   ¦   +---Properties
¦   ¦   L---Resources
¦   +---ServerFix
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerGrayBox
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---ServerIQ-DTN
¦   ¦   L---Properties
¦   +---ServerMB
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerNxCore
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerNxCore64
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerRealTick
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerRedi
¦   ¦   L---Properties
¦   +---ServerRithmic
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerSterling
¦   ¦   +---bin
¦   ¦   ¦   L---Release
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---ServerTD
¦   ¦   L---Properties
¦   +---ServerTDX
¦   ¦   +---ActiveXComp
¦   ¦   ¦   +---ActiveX
¦   ¦   ¦   L---DLL
¦   ¦   +---bin
¦   ¦   ¦   L---Debug
¦   ¦   +---obj
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---ServerZenfire
¦   ¦   +---dll
¦   ¦   L---Properties
¦   +---TikConverter
¦   ¦   L---Properties
¦   +---TikData
¦   +---TimeAndSales
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---TradeLinkAPI
¦   ¦   L---Properties
¦   +---TradeLinkAppKit
¦   ¦   +---Google
¦   ¦   L---Properties
¦   +---TradeLinkCommon
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---TradeLinkResearch
¦   ¦   +---bin
¦   ¦   ¦   +---Debug
¦   ¦   ¦   L---Release
¦   ¦   L---Properties
¦   +---TradeLinkTests
¦   ¦   L---Properties
¦   L---Warden
¦       L---Properties
L---wiki


ps: 
[q] why do I get lots of git warnings like this:
    warning: LF will be replaced by CRLF in ....
[a] use following command:
    git config core.autocrlf false