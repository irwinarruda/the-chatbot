import { useCallback, useState } from "react"
import { useKeyboard } from "@opentui/react"
import { useSSE } from "../hooks/useSSE.ts"
import { useApi } from "../hooks/useApi.ts"
import { useAutoCopy } from "../hooks/useAutoCopy.ts"
import { ChatMessages } from "./ChatMessages.tsx"
import { ChatInput } from "./ChatInput.tsx"
import { AudioPathInput } from "./AudioPathInput.tsx"
import { StatusBar } from "./StatusBar.tsx"
import { theme } from "../theme.ts"
import type { Message } from "../types.ts"

export function ChatView({ phoneNumber, baseUrl }: { phoneNumber: string; baseUrl: string }) {
  const [messages, setMessages] = useState<Message[]>([])
  const [audioStatus, setAudioStatus] = useState("")
  const [focusedInput, setFocusedInput] = useState<"text" | "audio">("text")
  useAutoCopy()

  const handleBotMessage = useCallback((msg: { Text: string; Buttons?: string[] }) => {
    setMessages((prev) => [
      ...prev,
      {
        role: "bot" as const,
        text: msg.Text,
        buttons: msg.Buttons,
        timestamp: new Date(),
      },
    ])
  }, [])

  const { connected, connecting } = useSSE(baseUrl, handleBotMessage)
  const { send, sendAudio } = useApi(baseUrl)

  useKeyboard((key) => {
    if (key.name === "tab") {
      setFocusedInput((focused) => focused === "text" ? "audio" : "text")
    }
  })

  const handleSendMessage = useCallback(
    async (text: string) => {
      if (!connected) return

      const userMessage: Message = {
        role: "user",
        text,
        timestamp: new Date(),
      }
      setMessages((prev) => [...prev, userMessage])

      try {
        await send(phoneNumber, text)
        setAudioStatus("")
      } catch (err) {
        console.error("Failed to send message:", err)
      }
    },
    [connected, phoneNumber, send],
  )

  const handleSendAudioPath = useCallback(
    async (filePath: string) => {
      if (!connected) return

      const userMessage: Message = {
        role: "user",
        text: `Audio: ${filePath}`,
        timestamp: new Date(),
      }
      setMessages((prev) => [...prev, userMessage])
      setAudioStatus("Uploading audio...")

      try {
        await sendAudio(phoneNumber, filePath)
        setAudioStatus("Audio sent for transcription")
      } catch (err) {
        setAudioStatus("Failed to send audio")
      }
    },
    [connected, phoneNumber, sendAudio],
  )

  return (
    <box
      flexGrow={1}
      flexDirection="column"
      backgroundColor={theme.neutral[950]}
    >
      <box
        flexDirection="row"
        alignItems="center"
        justifyContent="center"
        paddingLeft={2}
        paddingRight={2}
        height={3}
        flexShrink={0}
        border
        borderStyle="single"
        borderColor={theme.green[800]}
        backgroundColor={theme.neutral[900]}
      >
        <text fg={theme.green[500]}>
          <strong>{"  TheChatbot  "}</strong>
        </text>
      </box>

      <ChatMessages messages={messages} />

      <box
        flexDirection="column"
        flexShrink={0}
        border
        borderStyle="single"
        borderColor={theme.green[800]}
        backgroundColor={theme.neutral[900]}
      >
        <ChatInput
          onSend={handleSendMessage}
          disabled={!connected}
          focused={focusedInput === "text"}
        />
        <AudioPathInput
          onSend={handleSendAudioPath}
          disabled={!connected}
          focused={focusedInput === "audio"}
        />
        <StatusBar
          phoneNumber={phoneNumber}
          connected={connected}
          connecting={connecting}
          focusedInput={focusedInput}
          audioStatus={audioStatus}
        />
      </box>
    </box>
  )
}
