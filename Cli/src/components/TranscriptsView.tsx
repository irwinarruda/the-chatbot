import { useState, useEffect, useCallback } from "react"
import { useKeyboard } from "@opentui/react"
import { useApi } from "../hooks/useApi.ts"
import { theme } from "../theme.ts"
import type { Transcript } from "../types.ts"

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleString("en-US", {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  })
}

function truncate(text: string, maxLen: number): string {
  if (text.length <= maxLen) return text
  return text.slice(0, maxLen) + "..."
}

function formatMimeType(mime?: string): string {
  if (!mime) return ""
  const ext = mime.split("/").pop() ?? mime
  return `[${ext}]`
}

function TranscriptCard({
  transcript,
  selected,
}: {
  transcript: Transcript
  selected: boolean
}) {
  return (
    <box
      flexDirection="column"
      border
      borderStyle="single"
      borderColor={selected ? theme.green[500] : theme.neutral[700]}
      backgroundColor={selected ? theme.neutral[800] : theme.neutral[900]}
      paddingLeft={1}
      paddingRight={1}
      flexShrink={0}
    >
      <box flexDirection="row" justifyContent="space-between">
        <text fg={theme.neutral[300]}>
          {truncate(transcript.transcript, 70)}
        </text>
        <text fg={theme.neutral[500]}>
          {formatMimeType(transcript.mime_type)} {formatDate(transcript.created_at)}
        </text>
      </box>
    </box>
  )
}

function TranscriptDetail({
  transcript,
}: {
  transcript: Transcript
}) {
  return (
    <box
      flexGrow={1}
      flexDirection="column"
      justifyContent="center"
      alignItems="center"
      backgroundColor={theme.neutral[950]}
    >
      <box
        flexDirection="column"
        border
        borderStyle="rounded"
        borderColor={theme.green[600]}
        backgroundColor={theme.neutral[900]}
        padding={2}
        width="80%"
        gap={1}
      >
        <box flexDirection="row" justifyContent="space-between">
          <text fg={theme.green[400]}>
            <strong>Transcript Detail</strong>
          </text>
          <text fg={theme.neutral[500]}>
            {formatDate(transcript.created_at)}
          </text>
        </box>

        <box height={1} />

        <box flexDirection="column">
          <text fg={theme.neutral[200]}>
            {transcript.transcript}
          </text>
        </box>

        <box height={1} />

        {transcript.media_url ? (
          <box flexDirection="column" gap={0}>
            <text fg={theme.neutral[400]}>Download URL:</text>
            <text fg={theme.accent.cyan}>
              {transcript.media_url}
            </text>
          </box>
        ) : (
          <text fg={theme.neutral[600]}>No download URL available</text>
        )}

        <box height={1} />

        <box alignItems="center">
          <text fg={theme.neutral[600]}>
            Esc or Enter to close
          </text>
        </box>
      </box>
    </box>
  )
}

export function TranscriptsView({
  phoneNumber,
  baseUrl,
  onBack,
}: {
  phoneNumber: string
  baseUrl: string
  onBack: () => void
}) {
  const [transcripts, setTranscripts] = useState<Transcript[]>([])
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [detailIndex, setDetailIndex] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")
  const { getTranscripts } = useApi(baseUrl)

  const fetchTranscripts = useCallback(async () => {
    try {
      setLoading(true)
      setError("")
      var result = await getTranscripts(phoneNumber)
      setTranscripts(result)
    } catch (err) {
      setError("Failed to load transcripts")
    } finally {
      setLoading(false)
    }
  }, [phoneNumber, getTranscripts])

  useEffect(() => {
    fetchTranscripts()
  }, [fetchTranscripts])

  useKeyboard((key) => {
    if (detailIndex !== null) {
      if (key.name === "escape" || key.name === "return") {
        setDetailIndex(null)
      }
      return
    }

    if (key.ctrl && key.name === "t") {
      onBack()
      return
    }
    if (key.name === "escape") {
      onBack()
      return
    }
    if (key.name === "return" && transcripts.length > 0) {
      setDetailIndex(selectedIndex)
      return
    }
    if (key.name === "up") {
      setSelectedIndex((i) => Math.max(0, i - 1))
      return
    }
    if (key.name === "down") {
      setSelectedIndex((i) => Math.min(transcripts.length - 1, i + 1))
      return
    }
    if (key.name === "r") {
      fetchTranscripts()
      return
    }
  })

  if (detailIndex !== null && transcripts[detailIndex]) {
    return <TranscriptDetail transcript={transcripts[detailIndex]} />
  }

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
          <strong>{"  Transcripts  "}</strong>
        </text>
      </box>

      <box flexGrow={1} flexDirection="column">
        {loading ? (
          <box flexGrow={1} alignItems="center" justifyContent="center">
            <text fg={theme.neutral[400]}>Loading transcripts...</text>
          </box>
        ) : error ? (
          <box flexGrow={1} alignItems="center" justifyContent="center">
            <text fg={theme.accent.red}>{error}</text>
          </box>
        ) : transcripts.length === 0 ? (
          <box flexGrow={1} alignItems="center" justifyContent="center">
            <text fg={theme.neutral[500]}>No transcripts found</text>
          </box>
        ) : (
          <scrollbox stickyScroll stickyStart="top" flexGrow={1} flexDirection="column">
            {transcripts.map((t, i) => (
              <TranscriptCard
                key={t.id}
                transcript={t}
                selected={i === selectedIndex}
              />
            ))}
          </scrollbox>
        )}
      </box>

      <box
        flexDirection="row"
        justifyContent="space-between"
        alignItems="center"
        paddingLeft={2}
        paddingRight={2}
        height={1}
        flexShrink={0}
        border
        borderStyle="single"
        borderColor={theme.green[800]}
        backgroundColor={theme.neutral[900]}
      >
        <text fg={theme.neutral[600]}>{phoneNumber}</text>
        <text fg={theme.neutral[500]}>
          {transcripts.length} transcript{transcripts.length !== 1 ? "s" : ""}
        </text>
        <text fg={theme.neutral[700]}>
          Up/Down navigate | Enter detail | R refresh | Ctrl+T or Esc back
        </text>
      </box>
    </box>
  )
}
