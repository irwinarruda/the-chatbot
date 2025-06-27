# The Chatbot

## Authentication

- [ ] **Get login URL** Generate Google OAuth2 authorization URL.
- [ ] **Authenticate via Google** Exchange authorization code for access and refresh tokens.
- [ ] **Token Refresh** Automatically refresh expired access tokens.
- [ ] **Thank You Page** Redirect mobile users to a confirmation page after login.

## Sheets Integration

- [x] **Open Spreadsheet by URL** Fetch and cache spreadsheet metadata.
- [x] **Define Resources & Types** Model spreadsheet structures and enums for each sheet.
- [x] **Add Expense Entry** Append cost data to the daily log.
- [x] **Retrieve Data** Get the most recent entry. Calculate total spend for the current month.

## Task Management

- [ ] **Create Task** Add a new task with timestamp under a given task list.
- [ ] **Complete Task** Complete the task by it's name.
- [ ] **List Task Lists** Retrieve all available task lists.
- [ ] **List Tasks** Fetch all tasks from a specific task list.

## Messaging

- [ ] **Start Message** Send an initial greeting or menu.
- [ ] **Standard Message** Send plain text responses.
- [ ] **Option Message** Send choices or quick-reply buttons.
- [ ] **Receive & Process** Handle incoming user messages and dispatch to appropriate services.
