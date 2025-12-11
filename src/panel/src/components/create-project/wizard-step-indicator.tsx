import { Check } from "lucide-react"
import { cn } from "@/lib/utils"

interface Step {
  id: string
  title: string
  description: string
}

interface WizardStepIndicatorProps {
  steps: readonly Step[]
  currentStep: number
  onStepClick?: (index: number) => void
}

export function WizardStepIndicator({
  steps,
  currentStep,
  onStepClick,
}: WizardStepIndicatorProps) {
  return (
    <div className="flex items-center justify-between">
      {steps.map((step, index) => {
        const isComplete = index < currentStep
        const isCurrent = index === currentStep

        return (
          <div key={step.id} className="flex items-center">
            <button
              type="button"
              onClick={() => isComplete && onStepClick?.(index)}
              disabled={!isComplete}
              className={cn(
                "flex h-10 w-10 items-center justify-center rounded-full border-2 text-sm font-medium transition-colors",
                isComplete &&
                  "cursor-pointer border-primary bg-primary text-primary-foreground",
                isCurrent && "border-primary text-primary",
                !isComplete && !isCurrent && "border-muted text-muted-foreground"
              )}
            >
              {isComplete ? <Check className="h-5 w-5" /> : index + 1}
            </button>
            <div className="ml-3 hidden sm:block">
              <p
                className={cn(
                  "text-sm font-medium",
                  isCurrent ? "text-foreground" : "text-muted-foreground"
                )}
              >
                {step.title}
              </p>
            </div>
            {index < steps.length - 1 && (
              <div
                className={cn(
                  "mx-4 h-0.5 w-12 sm:w-24",
                  isComplete ? "bg-primary" : "bg-muted"
                )}
              />
            )}
          </div>
        )
      })}
    </div>
  )
}
