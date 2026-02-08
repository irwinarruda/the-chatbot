export interface Message {
  role: "user" | "bot"
  text: string
  buttons?: string[]
  timestamp?: Date
}

export interface SendMessageRequest {
  text: string
  phone_number: string
}

export interface TuiOutgoingMessage {
  Type: "text" | "button"
  Text: string
  To: string
  Buttons?: string[]
}

export interface SetupConfig {
  phoneNumber: string
  baseUrl: string
}
