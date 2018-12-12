node {
	stage 'Checkout'
		checkout scm

	stage 'Build'
	    bat "nuget restore"
		bat "\"${tool 'MSBuild'}\" EcoSignLift.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"

	stage 'Archive'
		bat "7z a EcoSignLift.zip Mods/"
		archiveArtifacts 'EcoSignLift.zip'
		cleanWs()
}