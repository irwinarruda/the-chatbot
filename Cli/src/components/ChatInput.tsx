import { useRef, useCallback, useMemo, useEffect } from "react"
import { theme } from "../theme.ts"
import type { TextareaRenderable } from "@opentui/core"

export function ChatInput({
  onSend,
  disabled,
  focused,
}: {
  onSend: (text: string) => void
  disabled: boolean
  focused?: boolean
}) {
  const textareaRef = useRef<TextareaRenderable>(null)

  const handleSubmit = useCallback(() => {
    if (!textareaRef.current || disabled) return
    const text = textareaRef.current.plainText.trim()
    if (!text) return
    onSend(text)
    textareaRef.current.clear()
  }, [disabled, onSend])

  useEffect(() => {
    if (!textareaRef.current) return
    textareaRef.current.onSubmit = handleSubmit
  }, [handleSubmit])

  const keyBindings = useMemo(
    () => [
      { name: "return", action: "submit" as const },
      { name: "return", shift: true, action: "newline" as const },
    ],
    [],
  )

  return (
    <box
      paddingLeft={1}
      paddingRight={1}
      minHeight={3}
      maxHeight={10}
      flexShrink={0}
      flexDirection="row"
      alignItems="flex-start"
      gap={1}
    >
      <text fg={theme.green[500]}>{"> "}</text>
      <textarea
        ref={textareaRef}
        flexGrow={1}
        placeholder={
          disabled ? "Waiting for connection..." : "Type a message..."
        }
        keyBindings={keyBindings}
        backgroundColor={theme.neutral[850]}
        focusedBackgroundColor={theme.neutral[800]}
        textColor={theme.neutral[100]}
        cursorColor={theme.green[400]}
        focused={focused}
      />
    </box>
  )
}
