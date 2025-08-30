# The Chatbot

```sh
dotnet tool install -g dotnet-ef
dotnet tool install -g dotnet-script
```

## Infra

- [x] **Automated Tests** Create integration tests project.
- [x] **Tests to CI** Integrate tests into Github Actions CI.
- [x] **Database** Implement database integration.
- [x] **Error** Implement generic error handling.
- [x] **Migration** Define migration infra.
- [x] **Architecture** Define the MVC like architecture where Services and Entities represent the Model.

## Status

- [x] **Status Endpoint** Create status endpoint that contains the following entries
- [x] **Database Status** Version, Max Connections, Open Connections, etc...
- [ ] **Messaging Setatus** WhatsApp Status

## Authentication

- [x] **Database Auth** Define auth structure in the database to support all kinds of products.
- [x] **Get login URL** Generate Google OAuth2 authorization URL.
- [x] **Authenticate via Google** Exchange authorization code for access and refresh tokens.
- [x] **Token Refresh** Automatically refresh expired access tokens.
- [x] **Thank You Page** Redirect mobile users to a confirmation page after login.

## Sheets Integration

- [x] **Open Spreadsheet by URL** Fetch and cache spreadsheet metadata.
- [x] **Define Resources & Types** Model spreadsheet structures and enums for each sheet.
- [x] **Add Expense Entry** Append cost data to the daily log.
- [x] **Retrieve Data** Get the most recent entry. Calculate total spend for the current month.

## Messaging

- [x] **Start Message** Send an initial greeting or menu.
- [x] **Standard Message** Send plain text responses.
- [x] **Option Message** Send choices or quick-reply buttons.
- [x] **Receive & Process** Handle incoming user messages and dispatch to appropriate services.

## Last tasks before v1

- [x] Fix URI ToString showing the port
- [ ] Create a welcome page
- [ ] Create a terms of use page
- [ ] Create a privacy policy page
- [ ] Create structure for allowed phone numbers
- [x] Create a way to reset chat keeping history
- [ ] Add the logo for this readme
- [ ] Add a readme for this project

## For later

- [ ] **Optimize Sheets** improve performance by using the database as a SOT.
- [ ] Refactor database with IDbContextFactory<AppDbContext> for concurrent work.
- [ ] Add tests forcing refresh token (refactor cashFlowGateway to work with token and credentials)

### Task Management

- [ ] **Create Task** Add a new task with timestamp under a given task list.
- [ ] **Complete Task** Complete the task by it's name.
- [ ] **List Task Lists** Retrieve all available task lists.
- [ ] **List Tasks** Fetch all tasks from a specific task list.
