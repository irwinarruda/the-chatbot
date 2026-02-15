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

export interface SendAudioRequest {
  phone_number: string
  file_path: string
  mime_type?: string
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

export interface Transcript {
  id: string
  transcript: string
  media_url?: string
  mime_type?: string
  created_at: string
}
