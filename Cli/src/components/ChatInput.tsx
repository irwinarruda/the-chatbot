import { useRef, useCallback } from "react"
import { theme } from "../theme.ts"
import type { InputRenderable } from "@opentui/core"

export function ChatInput({
  onSend,
  disabled,
  focused,
}: {
  onSend: (text: string) => void
  disabled: boolean
  focused?: boolean
}) {
  const inputRef = useRef<InputRenderable>(null)

  const handleSubmit = useCallback(
    (value: string) => {
      const text = value.trim()
      if (!text || disabled) return
      onSend(text)
      if (inputRef.current) {
        inputRef.current.value = ""
      }
    },
    [disabled, onSend],
  )

  return (
    <box
      paddingLeft={1}
      paddingRight={1}
      height={3}
      flexShrink={0}
      flexDirection="row"
      alignItems="center"
      gap={1}
    >
      <text fg={theme.green[500]}>{"> "}</text>
      <input
        ref={inputRef}
        flexGrow={1}
        placeholder={
          disabled ? "Waiting for connection..." : "Type a message..."
        }
        onSubmit={handleSubmit as unknown as undefined}
        backgroundColor={theme.neutral[850]}
        focusedBackgroundColor={theme.neutral[800]}
        textColor={theme.neutral[100]}
        cursorColor={theme.green[400]}
        focused={focused}
      />
    </box>
  )
}
