# read access token
$token = Get-content ./token.txt

# cloning frontend
git clone -b dev https://$token@github.com/EventRecommender/EventsRecommenderFrontend.git ./Controller/ClientApp

# clone services
git clone -b dev https://$token@github.com/EventRecommender/ActivityService ./activity
git clone -b dev https://$token@github.com/EventRecommender/UserManagerService ./usermanager
git clone -b Dev https://$token@github.com/EventRecommender/RecommenderService ./recommender
git clone  https://$token@github.com/EventRecommender/eventScraper.git ./scraper

docker-compose -f docker-composeJoined.yml up --build

# stop
Write-host "script ended"