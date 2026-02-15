import { useCallback } from "react"
import type { SendAudioRequest, SendMessageRequest, Transcript } from "../types.ts"

export function useApi(baseUrl: string) {
  const send = useCallback(
    async (phoneNumber: string, text: string) => {
      const request: SendMessageRequest = {
        text,
        phone_number: phoneNumber,
      }

      const response = await fetch(`${baseUrl}/api/v1/tui/messages`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
      })

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${await response.text()}`)
      }

      return await response.json()
    },
    [baseUrl],
  )

  const sendAudio = useCallback(
    async (phoneNumber: string, filePath: string) => {
      const request: SendAudioRequest = {
        phone_number: phoneNumber,
        file_path: filePath.trim(),
        mime_type: getMimeType(filePath),
      }

      const response = await fetch(`${baseUrl}/api/v1/tui/audio`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
      })

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${await response.text()}`)
      }

      return await response.json()
    },
    [baseUrl],
  )

  const getTranscripts = useCallback(
    async (phoneNumber: string): Promise<Transcript[]> => {
      const url = `${baseUrl}/api/v1/tui/transcripts?phoneNumber=${encodeURIComponent(phoneNumber)}`
      const response = await fetch(url)
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${await response.text()}`)
      }
      return await response.json() as Transcript[]
    },
    [baseUrl],
  )

  return { send, sendAudio, getTranscripts }
}

function getMimeType(filePath: string): string | undefined {
  const lowerPath = filePath.toLowerCase()
  if (lowerPath.endsWith(".ogg")) return "audio/ogg"
  if (lowerPath.endsWith(".flac")) return "audio/flac"
  if (lowerPath.endsWith(".wav")) return "audio/wav"
  if (lowerPath.endsWith(".mp3")) return "audio/mpeg"
  if (lowerPath.endsWith(".m4a")) return "audio/mp4"
  if (lowerPath.endsWith(".aac")) return "audio/aac"
  if (lowerPath.endsWith(".amr")) return "audio/amr"
  if (lowerPath.endsWith(".webm")) return "audio/webm"
  return undefined
}
