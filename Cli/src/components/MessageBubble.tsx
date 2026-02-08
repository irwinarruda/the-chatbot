import { theme } from "../theme.ts"
import type { Message } from "../types.ts"

export function MessageBubble({ message }: { message: Message }) {
  const isUser = message.role === "user"

  const time = message.timestamp
    ? `${String(message.timestamp.getHours()).padStart(2, "0")}:${String(message.timestamp.getMinutes()).padStart(2, "0")}`
    : ""

  return (
    <box
      paddingLeft={2}
      paddingRight={2}
      flexDirection="column"
    >
      <box flexDirection="row" gap={1}>
        <text fg={isUser ? theme.green[400] : theme.accent.cyan}>
          {isUser ? ">" : "<"}
        </text>
        <text fg={isUser ? theme.green[400] : theme.accent.cyan}>
          <strong>{isUser ? "You" : "Bot"}</strong>
        </text>
        <text fg={theme.neutral[600]}>{time}</text>
      </box>
      <box paddingLeft={4}>
        <text fg={isUser ? theme.neutral[200] : theme.neutral[300]}>
          {message.text}
        </text>
      </box>
      {message.buttons && message.buttons.length > 0 && (
        <box flexDirection="row" gap={2} paddingLeft={4} marginTop={1}>
          {message.buttons.map((btn, i) => (
            <text key={i} fg={theme.green[300]}>
              {"["}<strong>{i + 1}</strong>{"] "}{btn}
            </text>
          ))}
        </box>
      )}
      <box height={1} />
    </box>
  )
}
