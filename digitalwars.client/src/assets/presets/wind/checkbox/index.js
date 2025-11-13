export default {
  root: {
    class: [
      'relative',
      'inline-flex',
      'align-bottom',
      'w-6',
      'h-6',
      'cursor-pointer', 
      'select-none'
    ]
  },
  box: ({ props, context }) => ({
    class: [
      'flex',
      'items-center',
      'justify-center',
      'w-6',
      'h-6',
      'rounded',
      'border',
      'text-surface-600',
      'pointer-events-none',
      {
        'border-surface-300 bg-surface-0 dark:border-surface-700 dark:bg-surface-900':
          !context.checked && !props.invalid,
        'border-primary-500 bg-primary-500 dark:border-primary-400 dark:bg-primary-400':
          context.checked
      },
      { 'border-red-500 dark:border-red-400': props.invalid },
      {
        'ring-2 ring-primary-500 dark:ring-primary-400': !props.disabled && context.focused,
        'cursor-default opacity-60': props.disabled
      },
      {
        'peer-focus-visible:ring-2 peer-focus-visible:ring-primary-500 dark:peer-focus-visible:ring-primary-400':
          !props.disabled
      },
      'transition-colors',
      'duration-200'
    ]
  }),
  input: {
    class: [
      'peer',
      'w-full',
      'h-full',
      'absolute',
      'top-0 left-0',
      'z-10',
      'p-0',
      'm-0',
      'opacity-0',
      'rounded-md',
      'outline-none',
      'border-2 border-surface-300 dark:border-surface-700',
      'appearance-none',
      'cursor-pointer'  
    ]
  },
  icon: {
    class: [
      'text-base',
      'w-3',
      'h-3',
      'text-white dark:text-surface-900',
      'transition-all',
      'duration-200'
    ]
  }
};