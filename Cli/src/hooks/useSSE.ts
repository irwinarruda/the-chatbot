import { useEffect, useRef, useState } from "react"
import type { TuiOutgoingMessage } from "../types.ts"

export function useSSE(
  baseUrl: string,
  onMessage: (msg: TuiOutgoingMessage) => void,
) {
  const [connected, setConnected] = useState(false)
  const [connecting, setConnecting] = useState(false)

  const onMessageRef = useRef(onMessage)
  onMessageRef.current = onMessage

  useEffect(() => {
    let cancelled = false
    let retryCount = 0
    const maxRetries = 5
    const abortController = new AbortController()

    async function connect() {
      if (cancelled) return

      setConnecting(true)
      try {
        const url = `${baseUrl}/api/v1/tui/messages/stream`
        const response = await fetch(url, { signal: abortController.signal })

        if (!response.ok) {
          const body = await response.text().catch(() => "")
          throw new Error(`HTTP ${response.status}: ${body}`)
        }

        setConnected(true)
        setConnecting(false)
        retryCount = 0

        const reader = response.body?.getReader()
        if (!reader) throw new Error("No response body reader")

        const decoder = new TextDecoder()
        let buffer = ""

        while (true) {
          const { done, value } = await reader.read()
          if (done) break
          if (cancelled) {
            reader.cancel()
            break
          }

          buffer += decoder.decode(value, { stream: true })
          const lines = buffer.split("\n")
          buffer = lines[lines.length - 1]!

          for (let i = 0; i < lines.length - 1; i++) {
            const line = lines[i]!.trim()
            if (line.startsWith("data: ")) {
              try {
                const json = JSON.parse(line.slice(6))
                onMessageRef.current(json)
              } catch {}
            }
          }
        }
      } catch (err) {
        if (cancelled) return
      } finally {
        if (!cancelled) {
          setConnected(false)
          setConnecting(false)
        }
      }

      if (retryCount < maxRetries && !cancelled) {
        retryCount++
        const delay = Math.min(1000 * Math.pow(2, retryCount - 1), 10000)
        setTimeout(connect, delay)
      }
    }

    connect()

    return () => {
      cancelled = true
      abortController.abort()
      setConnected(false)
      setConnecting(false)
    }
  }, [baseUrl])

  return { connected, connecting }
}
