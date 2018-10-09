pipeline {

	agent none
	stages {
		stage('Build everything')
		{
			agent { label 'windows' }
			steps {
				dir('artifact'){
					deleteDir()
				}
				
				dir('src'){
					bat '''
					@echo off
					dotnet publish OneScript/OneScriptWeb.csproj -c Release -f net461 -o ../../artifact/net461/win7-x64 -r win7-x64
					dotnet publish OneScript/OneScriptWeb.csproj -c Release -f net461 -o ../../artifact/net461/debian-x64 -r debian-x64
					'''
				}
				
				// новые версии дженкинса падают, если есть ранее зипованый артефакт
				fileOperations([fileDeleteOperation(excludes: '', includes: '*.zip')])
				
				zip archive: true, dir: 'artifact/net461/win7-x64', glob: '', zipFile: 'oscript.web-win7-x64.zip'
				zip archive: true, dir: 'artifact/net461/debian-x64', glob: '', zipFile: 'oscript.web-debian-x64.zip'
				
				dir('src/OneScriptWeb.Tests'){
					bat '''
					@echo off
					dotnet restore
					dotnet xunit -nunit testresult.xml -configuration Release
					'''
					
					nunit testResultsPattern: 'testresult.xml'
				}

			}
		}
	}
}