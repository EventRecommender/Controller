# cloning frontend
git clone https://github.com/EventRecommender/EventsRecommenderFrontend.git ./Controller/ClientApp

# clone services
git clone https://github.com/EventRecommender/ActivityService ./activity
git clone https://github.com/EventRecommender/UserManagerService ./usermanager
git clone https://github.com/EventRecommender/RecommenderService ./recommender
git clone https://github.com/EventRecommender/eventScraper.git ./scraper

docker-compose -f docker-composeSingle.yml up --build

# stop
Write-host "script ended"