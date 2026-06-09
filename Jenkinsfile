pipeline {
    agent any

    environment {
        // Docker Hub Registry Targets
        DOCKER_USER     = 'shubd0cker'       
        IMAGE_NAME      = 'product-catalog-api'
        IMAGE_TAG       = "${BUILD_NUMBER}" 
        DOCKER_CREDS_ID = 'docker-hub-credentials-id'
        
        // Decoupled GitOps Infrastructure Configuration Repository
        GITHUB_CREDS_ID = 'github-gitops-token-id'
        INFRA_REPO      = 'github.com/shubhamroy-git/gitops-infra-config.git' 
    }

    stages {

        stage('1. Compile, Test & Analyze') {
            steps {
                // We use the encrypted secret token we saved inside Jenkins' vault
                withCredentials([string(credentialsId: 'SONAR_TOKEN', variable: 'SONAR_TOKEN')]) {
                    
                    // Explicitly run everything inside our .NET 10 SDK environment
                    sh '''
                        echo "=== Installing Sonar Scanner Tool ==="
                        dotnet tool install --global dotnet-sonarscanner --version 9.x || true
                        export PATH="$PATH:$HOME/.dotnet/tools"

                        echo "=== Beginning Sonar Qube Analysis Block ==="
                        dotnet sonarscanner begin \
                        /k:"product-catalog-api" \
                        /d:sonar.token="$SONAR_TOKEN" \
                        /d:sonar.host.url="http://localhost:9000" \

                        echo "=== Compiling Application Binaries ==="
                        dotnet build --configuration Release

                        echo "=== Finalizing Analysis & Shipping Metrics ==="
                        dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
                    '''
                }
            }
        }
        

        stage('2. Build Container Image') {
            steps {
                // Executes your optimized chiseled runtime Dockerfile
                sh "docker build -t ${DOCKER_USER}/${IMAGE_NAME}:${IMAGE_TAG} ."
                sh "docker tag ${DOCKER_USER}/${IMAGE_NAME}:${IMAGE_TAG} ${DOCKER_USER}/${IMAGE_NAME}:latest"
            }
        }

        stage('3. Push to Docker Hub') {
            steps {
                // Securely logs into Docker Hub using credentials masked from the Jenkins vault
                withCredentials([usernamePassword(credentialsId: "${DOCKER_CREDS_ID}", passwordVariable: 'DOCKER_PASSWORD', usernameVariable: 'DOCKER_USERNAME')]) {
                    sh "echo \$DOCKER_PASSWORD | docker login -u \$DOCKER_USERNAME --password-stdin"
                }
                echo "Publishing container image versions to Docker Hub..."
                sh "docker push ${DOCKER_USER}/${IMAGE_NAME}:${IMAGE_TAG}"
                sh "docker push ${DOCKER_USER}/${IMAGE_NAME}:latest"
            }
        }

        stage('4. Update GitOps Manifest Repository') {
            steps {
                // Extracts your GitHub PAT to securely update your isolated infrastructure repo
                withCredentials([usernamePassword(credentialsId: "${GITHUB_CREDS_ID}", passwordVariable: 'GITHUB_TOKEN', usernameVariable: 'GITHUB_USER')]) {
                    sh """
                        # Clear any old temporary checkouts and clone a fresh state of the infrastructure repo

                        rm -rf gitops-infra-config
                        git clone https://${GITHUB_USER}:${GITHUB_TOKEN}@${INFRA_REPO}
                        cd gitops-infra-config
                        
                        # Use sed text replacement to update the image tag to the current build number

                        sed -i 's|image: .*product-catalog-api:.*|image: ${DOCKER_USER}/${IMAGE_NAME}:${IMAGE_TAG}|g' deployment.yaml
                        
                        # Configure local bot author identity and push the change back to GitHub
                        
                        git config user.name "Jenkins Automation"
                        git config user.email "jenkins@shubd0cker.dev"
                        git add deployment.yaml
                        git commit -m "gitops: update product-catalog-api to version ${IMAGE_TAG}"
                        git push origin main
                    """
                }
                echo "Successfully shifted manifest state inside gitops-infra-config!"
            }
        }
    }

    post {
        success {
            echo "======================================================="
            echo "Pipeline Build #${BUILD_NUMBER} SUCCESSFUL! GitOps state updated cleanly."
            echo "======================================================="
        }
        failure {
            echo "======================================================="
            echo "Pipeline Build #${BUILD_NUMBER} FAILED. Review console output logs."
            echo "======================================================="
        }
    }
}