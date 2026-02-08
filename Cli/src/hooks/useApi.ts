import { useCallback } from "react"
import type { SendMessageRequest } from "../types.ts"

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

  return { send }
}
