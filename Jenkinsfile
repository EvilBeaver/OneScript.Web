pipeline {

	agent none
	stages {
		stage('Build everything')
		{
			options { skipDefaultCheckout() }
            agent { 
                label 'windows'
            }
			steps {
				checkout(
                    [$class: 'GitSCM', branches: [[name: "${env.BRANCH_NAME}"]],
                     doGenerateSubmoduleConfigurations: false,
                     extensions: [
                         [$class: 'SubmoduleOption', 
                         disableSubmodules: false,
                         parentCredentials: false,
                         recursiveSubmodules: true,
                         reference: '',
                         trackingSubmodules: false]],
                         submoduleCfg: [],
                         userRemoteConfigs: [[url: 'https://github.com/EvilBeaver/OneScript.Web.git']]])

                /*dir('src/OneScriptWeb.Tests'){
					bat '''
					@echo off
					dotnet restore
					dotnet xunit -nunit testresult.xml -configuration Release
					'''
					
					nunit testResultsPattern: 'testresult.xml'
				}*/

				dir('artifact'){
					deleteDir()
				}
				
				dir('src'){
					bat '''
					@echo off
					dotnet publish OneScript/OneScriptWeb.csproj -c Release -f net461 -o ../../artifact/net461/win7-x64 -r win7-x64
					dotnet publish OneScript/OneScriptWeb.csproj -c Release -f net461 -o ../../artifact/net461/debian-x64 -r debian-x64
					dotnet publish OneScript/OneScriptWeb.csproj -c Release -f netcoreapp2.2 -o ../../artifact/core22/debian-x64 -r debian-x64
					'''
				}
				
				// новые версии дженкинса падают, если есть ранее зипованый артефакт
				fileOperations([fileDeleteOperation(excludes: '', includes: '*.zip')])
				
				zip archive: true, dir: 'artifact/net461/win7-x64', glob: '', zipFile: 'oscript.web-win7-x64.zip'
				zip archive: true, dir: 'artifact/net461/debian-x64', glob: '', zipFile: 'oscript.web-debian-x64.zip'
				zip archive: true, dir: 'artifact/core22/debian-x64', glob: '', zipFile: 'oscript.web-debian-x64-core.zip'
			}
		}
		stage('Create docker image'){
			options { skipDefaultCheckout() }
            agent { 
                label 'linux'
            }
			steps {
				checkout(
                    [$class: 'GitSCM', branches: [[name: "${env.BRANCH_NAME}"]],
                     doGenerateSubmoduleConfigurations: false,
                     extensions: [
                         [$class: 'SubmoduleOption', 
                         disableSubmodules: false,
                         parentCredentials: false,
                         recursiveSubmodules: true,
                         reference: '',
                         trackingSubmodules: false]],
                         submoduleCfg: [],
                         userRemoteConfigs: [[url: 'https://github.com/EvilBeaver/OneScript.Web.git']]])

				withCredentials([usernamePassword(credentialsId: 'docker-hub', passwordVariable: 'dockerpassword', usernameVariable: 'dockeruser')]) {
					sh 'docker build -t evilbeaver/oscript-web:0.5.0 --file Dockerfile src'
					sh 'docker login -p $dockerpassword -u $dockeruser && docker push'
				}
			}			
		}
	}
}