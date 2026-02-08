import { MessageBubble } from "./MessageBubble.tsx"
import { theme } from "../theme.ts"
import type { Message } from "../types.ts"

export function ChatMessages({ messages }: { messages: Message[] }) {
  return (
    <scrollbox
      stickyScroll
      stickyStart="bottom"
      backgroundColor={theme.neutral[950]}
    >
      {messages.length === 0 ? (
        <box
          paddingTop={3}
          paddingBottom={3}
          alignItems="center"
          flexDirection="column"
          gap={1}
        >
          <text fg={theme.green[700]}>{"~ ~ ~"}</text>
          <text fg={theme.neutral[400]}>No messages yet</text>
          <text fg={theme.neutral[600]}>Type below to start the conversation</text>
        </box>
      ) : (
        <box flexDirection="column" paddingTop={1} paddingBottom={1}>
          {messages.map((msg, i) => <MessageBubble key={i} message={msg} />)}
        </box>
      )}
    </scrollbox>
  )
}
