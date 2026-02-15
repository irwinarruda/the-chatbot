import { theme } from "../theme.ts"

export function StatusBar({
  phoneNumber,
  connected,
  connecting,
  focusedInput,
  audioStatus,
}: {
  phoneNumber: string
  connected: boolean
  connecting: boolean
  focusedInput: "text" | "audio"
  audioStatus: string
}) {
  let statusIcon: string
  let statusText: string
  let statusColor: string

  if (connecting) {
    statusIcon = "~"
    statusText = "Connecting"
    statusColor = theme.accent.amber
  } else if (connected) {
    statusIcon = "*"
    statusText = "Connected"
    statusColor = theme.green[500]
  } else {
    statusIcon = "x"
    statusText = "Disconnected"
    statusColor = theme.accent.red
  }

  return (
    <box
      flexDirection="row"
      justifyContent="space-between"
      alignItems="center"
      paddingLeft={2}
      paddingRight={2}
      height={1}
      flexShrink={0}
    >
      <text fg={theme.neutral[600]}>{phoneNumber}</text>
      <text fg={statusColor}>
        {statusIcon} {statusText}
      </text>
      <text fg={theme.neutral[700]}>
        {focusedInput === "text" ? "Input: text" : "Input: audio"} | {audioStatus || "Tab switch"} | Ctrl+T transcripts | Ctrl+C exit
      </text>
    </box>
  )
}
