# Spotify Slack Listener
Combines Spotify and Slack Api to update your Slack status with your current playing song on Spotify.

- [Slack Api](https://api.slack.com/web)
- [Spotify Api](https://developer.spotify.com/documentation/web-api/reference/)
- [Demo](https://www.slackspotifylistener.com) - A demo of this project, it's on a really lightweight server to please don't overload. App is unapproved on Slack.

## Setup Instructions

#### Create apps for Slack & Spotify
Create an app on Slack: https://api.slack.com/apps

Create an app on Spotify: https://developer.spotify.com/dashboard/applications

Add the valid redirect uris:  
Slack: `https://localhost:5001/slack-callback`  
Spotify: `https://localhost:5001/spotify-callback`

#### Update project with your client id & secrets
Update app settings to include your client ids and secrets, you can modify the `appsettings.json`, update user secrets, or set environment variables.

```json
"Slack": {
  "ClientId": "<Insert Slack Client Id>",
  "ClientSecret": "<Insert Slack Client Secret>"
},
"Spotify": {
  "ClientId": "<Insert Spotify Client Id>",
  "ClientSecret": "<Insert Spotify Client Secret>"
}
```
#### Create SqlServer database and update connection string
I've included a `docker-compose` file to create a SqlServer locally on port 5433. Start the instance by `docker-compose up`. If you wish to use your own database you must update the connection string in the app settings.

Migrations are automatic, and it will generate the users table. 

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,5433;Database=SpotifySlackListener;User Id=sa;Password=Pass@word"
},
```

#### Launch website
Website is bound to http port 5000 and https port 5001 for development, port 80/443 for production.
https://localhost:5001/

