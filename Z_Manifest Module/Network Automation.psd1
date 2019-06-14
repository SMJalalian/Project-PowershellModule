#
# Module manifest for module 'module'
#
# Generated by: Mohamad
#
# Generated on: 6/6/2019 2:35:58 PM
#

@{

# Script module or binary module file associated with this manifest.
# RootModule = 'Manifest Module.psm1'

# Version number of this module.
ModuleVersion = '1.0'

# ID used to uniquely identify this module
GUID = 'd86466d9-88a7-4029-95b6-ea954420f0c8'

# Author of this module
Author = 'Mohamad Jalalian'

# Company or vendor of this module
CompanyName = 'Parmis Network Development'

# Copyright statement for this module
Copyright = '(c) 2019 Mohamad. All rights reserved.'

# Supported PSEditions
CompatiblePSEditions = @('Core')

# Description of the functionality provided by this module
# Description = ''

# Minimum version of the Windows PowerShell engine required by this module
# PowerShellVersion = ''

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = @('.\netcoreapp2.2\Microsoft Automation.dll',
				  '.\netcoreapp2.2\Cisco Automation.dll')

# Functions to export from this module
FunctionsToExport = '*'

# Cmdlets to export from this module
CmdletsToExport = 'Send-NACiscoCommand','Show-NACiscoInterfaceDescription','Show-NACiscoIPInterfaceBrief','Show-NACiscoARP',
				  'Get-NAUser','Get-NAGroup',
				  'Disable-NAInactiveComputer','Disable-NAInactiveUser'

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module
AliasesToExport = '*'

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess
# PrivateData = ''

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}

