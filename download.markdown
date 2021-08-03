---
layout: page
title: Download
permalink: /download/
nav_order: 2
---

For instructions on how to use dotSetup, please refer to our [FAQ](/faq.markdown) and [Wiki](https://github.com/dotsetup/dotsetup/wiki).

### Get latest dotSetup stable version:
- [https://github.com/dotsetup/dotsetup/releases/](https://github.com/dotsetup/dotsetup/releases/)

### Get latest dotSetup version from beta channel:
– Not available yet

### dotSetup v1.90.0.7828 release notes (stable):
- new util classes: AssemblyUtils, DateTimeUtils, GraphicsUtils
- dynamic session data, calculated on demand. data is stored in a dictionary, instead of the XML itself, but is accessible through the XML (via xpaths)
- new UI components extensions: ButtonEx, CheckBoxEx, Divider, ProgressBarEx
- parse products on DotSetup initialization
- CommunicationUtils - new methos for sending http async requests (post \ get)
- refactor namespaces all over the project
- fixed bug with running msi files
- ProcessUtils - fixed bug in "Parent" extension: crashed when the parent is already dead
- RegistryUtils - open hive according to specified view, allow regex in IsRegKeyExists, new method for writing registry keys, fixed bug in the function StringToNumberString
- UriUtils - update GetChromeExe according to Google GCAPI.dll
- XmlParser - new event OnXpathProcessing for additional logic on Xpaths
- XmlProcessor now supports values (aka right operand)
- XmlParser - new method:GetChildNodesValues - returns the direct siblings of specified node as a dictionary
- XmlProcessor - new processors for numbers and dates (add\sub)
- added the ability to update product's settings on run time (by updating the xml)
- new event mechanism for products:
- actions: HttpGetRequestOn, HttpPostRequestOn, WriteRegistryKeyOn, RunOn
- new session data values: TodayYYYYMMDD (today's date in yyyymmdd format), UnixTimeMs (session's start time in unix format), browsers paths (for ie, edge, chrome, firefox, opera if installed)
- new states for packages: CheckStart, CheckPassed, Displayed, AppClose, ProcessExecute (for executions unrelated to the execution of the download file)
- PackageDownloader - added the download time in MS to the custom data of the product in the XML
- added new XML Boolean attribute for a product: "exclusive" - if true, its requirement can be fulfilled only if it the only optional product in the flow (failed if a previous product passed, and if passed it fails any following optional product)
- added new dynamic tag to Product's custom data - "RunData", contains the exit code the runtime duration in MS
- RequirementHandlers - fixed bugs in results handler
- lot of minor tweaks
