# read access token
$token = Get-content ./token.txt

# cloning frontend
git clone -b dev https://$token@github.com/EventRecommender/EventsRecommenderFrontend.git ./Controller/ClientApp

# clone services
git clone -b dev https://$token@github.com/EventRecommender/ActivityService ./activity
git clone -b dev https://$token@github.com/EventRecommender/UserManagerService ./usermanager
git clone -b Dev https://$token@github.com/EventRecommender/RecommenderService ./recommender

# start each service
docker-compose -f ./activity/docker-compose.yml up -d --build
docker-compose -f ./usermanager/UserManager/docker-compose.yml up -d --build  # might change <-
docker-compose -f ./recommender/docker-compose.yml up -d --build

# building
docker build . --tag controller
docker run -d --name controller-service -p 80:80 controller

# stop
Write-host "script ended"