$token = Get-content ./token.txt

# cloning frontend
git clone -b dev https://$token@github.com/EventRecommender/EventsRecommenderFrontend.git ./Controller/ClientApp

docker build . --tag controllerdebug

docker run -p 80:80 controllerdebug