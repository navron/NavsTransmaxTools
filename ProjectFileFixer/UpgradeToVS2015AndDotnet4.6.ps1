#######################################################################################################################
# THIS SCRIPT CAN UPGRADE ONE OR MORE VISUAL STUDIO 2008 C# PROJECTS TO VISUAL STUDIO 2015 .NET FRAMEWORK VERSION 4.6.2
#######################################################################################################################
param([string]$sourcepath=$args[0])

# TargetFrameworkVersion
# NOTE: You can change the version of projects by specifying a different version. This will upgrade all projects not at this version.
$targetFramework = "v4.6.2"

###########
# Functions
###########

# Check if we need to upgrade a CSharp project and return $true if so, $false otherwise
function Check-NeedToUpgrade($projectFileNameFull)
{
    $needToUpgrade = $false

    # Load Xml Content
    $xmlProj = [xml](Get-Content $projectFileNameFull)
    
    if ($xmlProj.Project -ne $null)
    {
        $toolsVersion = $xmlProj.Project.GetAttributeNode("ToolsVersion")
        if ($toolsVersion -ne $null -and [version]$toolsVersion.Value -ne [version]"14.0")
        {            
            $needToUpgrade = $true;
        }
        $frameworkVersion =  $xmlProj.GetElementsByTagName("TargetFrameworkVersion") | Select-Object -first 1
        if($frameworkVersion -ne $null -and $frameworkVersion.InnerText -ne $targetFramework)
        {            
            $needToUpgrade = $true;
        }
    }
    else
    {
        $productVersion = $xmlProj.VisualStudioProject.CSHARP.GetAttributeNode("ProductVersion")

        if ($productVersion -ne $null -and [version]$productVersion.Value -lt [version]"11.0.0")
        {
            $needToUpgrade = $true;
        }
    }

    return $needToUpgrade
}


# Upgrade visual studio 2012 cs project to visual studio 2013
function Upgrade-Project($projectFileNameFull)
{
	Write-Host "Processing " $projectFileNameFull


    if ((Check-NeedToUpgrade($projectFileNameFull)) -eq $false) 
    {
        Write-Host "Already upgraded $projectFileNameFull"
        return
    }   
    
    # Load Xml Content
    $xmlProj = [xml](Get-Content $projectFileNameFull)

    if ($xmlProj.Project -eq $null)
    {
        Write-Host "No project file found at $projectFileNameFull"
        return
    }

    # Update Target Framework Version
    $projGroups = $xmlProj.Project.PropertyGroup     	
    $upgraded = $false
    foreach ($projGroup in $projGroups | Select-Object -first 1)
    {
        # Upgrade Tools Version from 4.0 to 12.0
        $toolsVersion = $xmlProj.Project.GetAttributeNode("ToolsVersion")
        $toolsVersion.InnerText = "14.0"

        $targetFrameworkElements = $projGroup.GetElementsByTagName("TargetFrameworkVersion") | Select-Object -first 1

        #make sure we have a targetframework element
        if($targetFrameworkElements.Count -eq 0){
            $firstGroup = $projGroups[0];
            $targetFrameworkElement = $xmlProj.CreateElement("TargetFrameworkVersion","http://schemas.microsoft.com/developer/msbuild/2003");
            $firstGroup.AppendChild($targetFrameworkElement);

            #Reread the targetframeworkelements
            $targetFrameworkElements = $projGroup.GetElementsByTagName("TargetFrameworkVersion") | Select-Object -first 1
        }                

        foreach($targetFrameworkElement in $targetFrameworkElements)
        {
            # Now we need to set the Target Framework Version to new version
            $targetFrameworkElement.InnerText = $targetFramework
        }        
    }  
    Save-Project $projectFileNameFull $xmlProj 
}

# Save XML Content to a CSharp Project file 
function Save-Project($projectFileName, [xml]$xmlContent)
{
    Write-Host "Saving project $projectFileName"
    $stringWriter = New-Object System.IO.StringWriter
    $xmlWriter = New-Object System.Xml.XmlTextWriter $stringWriter
    $xmlWriter.Formatting = "indented"
    $xmlWriter.Indentation = 4
    $xmlContent.WriteContentTo($xmlWriter)
    $xmlWriter.Flush()
    $stringWriter.Flush()
    $xmlContent = [xml]$stringWriter.ToString()
    $xmlContent.Save($projectFileName)

}

#############
# Script Main
#############

$projFiles = ls $sourcePath -filter "*.csproj"  -recurse

foreach ($proj in $projFiles)
{
	Upgrade-Project($proj.FullName)
}   


