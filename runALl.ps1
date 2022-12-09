# read access token
$token = Get-content ./token.txt

# cloning frontend
git clone https://$token@github.com/EventRecommender/EventsRecommenderFrontend.git ./Controller/ClientApp

# clone services
git clone https://$token@github.com/EventRecommender/ActivityService ./activity
git clone https://$token@github.com/EventRecommender/UserManagerService ./usermanager
git clone https://$token@github.com/EventRecommender/RecommenderService ./recommender

# start each service
docker-compose -f ./activity/docker-compose.yml up -d 
docker-compose -f ./usermanager/docker-compose.yml up -d  # might change <-
docker-compose -f ./recommender/docker-compose.yml up -d 

# stop
Write-host "script ended"