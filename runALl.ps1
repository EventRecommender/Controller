# read access token
$token = Get-content ./token.txt

# clone repoes
git clone https://$token@github.com/EventRecommender/ActivityService ./activity
git clone https://$token@github.com/EventRecommender/UserManagerService ./usermanager
git clone https://$token@github.com/EventRecommender/RecommenderService ./recommender

# start each service
docker-compose -f ./activity/docker-compose.yml -d up
docker-compose -f ./usermanager/docker-compose.yml -d up # might change <-
docker-compose -f ./recommender/docker-compose.yml -d up

# stop
Write-host "script ended"