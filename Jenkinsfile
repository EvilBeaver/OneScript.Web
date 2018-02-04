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
					dotnet publish OneScript/OneScriptWeb.csproj -c Release -o ../../artifact -r win7-x64
					'''
				}
				
				dir('src/OneScriptWeb.Tests'){
					bat '''
					@echo off
					dotnet restore
					dotnet xunit -nunit testresult.xml -configuration Release
					'''
					
					nunit testResultsPattern: 'testresult.xml'
				}
				
				archiveArtifacts 'artifact/**'
			}
		}
	}
}