pipeline {

	agent none
  
	environment {
        ReleaseNumber = '0.9.3'
    }
	stages {

		stage("Test platforms"){
			options { skipDefaultCheckout() }
			parallel{
				stage('Test Windows'){
					agent{ label "windows"}
					
					steps{
						checkout(
							[$class: 'GitSCM', branches: [[name: "${env.BRANCH_NAME}"]],
							doGenerateSubmoduleConfigurations: false,
							extensions: [
								[$class: 'SubmoduleOption', 
								disableSubmodules: false,
								parentCredentials: false,
								recursiveSubmodules: false,
								reference: '',
								trackingSubmodules: false]],
								submoduleCfg: [],
								userRemoteConfigs: [[url: 'https://github.com/EvilBeaver/OneScript.Web.git']]])
						
						bat '''
						dotnet test src/OneScriptWeb.Tests/OneScriptWeb.Tests.csproj ^
							-c Release ^
							-f netcoreapp3.1 ^
							--runtime win-x64 --logger="trx;LogFileName=win.trx" --results-directory=testResults
						'''.stripIndent()

						mstest testResultsFile: 'testResults/*.trx'
					}
				}

				stage('Test Linux') {

					agent {
						docker {
							image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
							label 'linux'
						}
					}
					environment {
						DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
					}

					steps{
						checkout(
							[$class: 'GitSCM', branches: [[name: "${env.BRANCH_NAME}"]],
							doGenerateSubmoduleConfigurations: false,
							extensions: [
								[$class: 'SubmoduleOption', 
								disableSubmodules: false,
								parentCredentials: false,
								recursiveSubmodules: false,
								reference: '',
								trackingSubmodules: false]],
								submoduleCfg: [],
								userRemoteConfigs: [[url: 'https://github.com/EvilBeaver/OneScript.Web.git']]])

						sh '''
						dotnet test src/OneScriptWeb.Tests/OneScriptWeb.Tests.csproj \
							-c Release \
							-f netcoreapp3.1 \
							--runtime linux-x64 --logger="trx;LogFileName=linux.trx" --results-directory=testResults
						'''.stripIndent()

						mstest testResultsFile: 'testResults/*.trx'
					}
				}
			}
		}
		

		stage('Publish'){
			options { skipDefaultCheckout() }
			agent {
				docker {
					image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
					label 'linux'
				}
			}
			environment {
				DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
			}
			steps {
				
				deleteDir()
				checkout(
                    [$class: 'GitSCM', branches: [[name: "${env.BRANCH_NAME}"]],
                     doGenerateSubmoduleConfigurations: false,
                     extensions: [
                         [$class: 'SubmoduleOption', 
                         disableSubmodules: false,
                         parentCredentials: false,
                         recursiveSubmodules: false,
                         reference: '',
                         trackingSubmodules: false]],
                         submoduleCfg: [],
                         userRemoteConfigs: [[url: 'https://github.com/EvilBeaver/OneScript.Web.git']]])
				
				sh 'dotnet clean src/OneScript.sln'
				sh '''dotnet publish src/OneScript/OneScriptWeb.csproj \
					/p:ReleaseNumber=${ReleaseNumber} \
					-c Release \
					-f netcoreapp3.1 \
					-r win-x64 \
					-o artifact/core/win-x64

				'''.stripIndent()

				sh '''dotnet publish src/OneScript/OneScriptWeb.csproj \
					/p:ReleaseNumber=${ReleaseNumber} \
					-c Release \
					-f netcoreapp3.1 \
					-r linux-x64 \
					-o artifact/core/linux-x64

				'''.stripIndent()

				stash includes: 'artifact/**', name: 'buildResults'
				
				// новые версии дженкинса падают, если есть ранее зипованый артефакт
				fileOperations([fileDeleteOperation(excludes: '', includes: '*.zip')])
				
				zip archive: true, dir: 'artifact/core/win-x64', glob: '', zipFile: 'oscript.web-win-x64.zip'
				zip archive: true, dir: 'artifact/core/linux-x64', glob: '', zipFile: 'oscript.web-linux-x64.zip'
			}
		}

		stage('Create docker image') {
			when { branch 'master' }
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
                         recursiveSubmodules: false,
                         reference: '',
                         trackingSubmodules: false]],
                         submoduleCfg: [],
                         userRemoteConfigs: [[url: 'https://github.com/EvilBeaver/OneScript.Web.git']]])

				unstash 'buildResults'

				withCredentials([usernamePassword(credentialsId: 'docker-hub', passwordVariable: 'dockerpassword', usernameVariable: 'dockeruser')]) {
				sh "docker build -t evilbeaver/oscript-web:${ReleaseNumber} --file Dockerfile artifact/core/linux-x64"
				sh "docker tag evilbeaver/oscript-web:${ReleaseNumber} evilbeaver/oscript-web:latest"
				sh "docker login -p $dockerpassword -u $dockeruser && docker push evilbeaver/oscript-web:${ReleaseNumber} && docker push evilbeaver/oscript-web:latest"
				}
			}			
		}
	}
}