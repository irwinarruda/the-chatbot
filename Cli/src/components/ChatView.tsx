import { useCallback, useState } from "react"
import { useSSE } from "../hooks/useSSE.ts"
import { useApi } from "../hooks/useApi.ts"
import { useAutoCopy } from "../hooks/useAutoCopy.ts"
import { ChatMessages } from "./ChatMessages.tsx"
import { ChatInput } from "./ChatInput.tsx"
import { StatusBar } from "./StatusBar.tsx"
import { theme } from "../theme.ts"
import type { Message } from "../types.ts"

export function ChatView({ phoneNumber, baseUrl }: { phoneNumber: string; baseUrl: string }) {
  const [messages, setMessages] = useState<Message[]>([])
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
  const { send } = useApi(baseUrl)

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
      } catch (err) {
        console.error("Failed to send message:", err)
      }
    },
    [connected, phoneNumber, send],
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
        <ChatInput onSend={handleSendMessage} disabled={!connected} focused />
        <StatusBar phoneNumber={phoneNumber} connected={connected} connecting={connecting} />
      </box>
    </box>
  )
}
