import React from 'react';

interface DijaLogoProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export default function DijaLogo({ size = 'md', className = '' }: DijaLogoProps) {
  const sizeClasses = {
    sm: 'h-8 w-8 text-2xl',
    md: 'h-10 w-10 text-3xl',
    lg: 'h-16 w-16 text-5xl'
  };

  return (
    <div className={`${sizeClasses[size]} ${className}`}>
      <div className="relative flex items-center justify-center h-full w-full">
        {/* Main logo circle with golden gradient */}
        <div className="absolute inset-0 rounded-full bg-gradient-to-br from-[#F4E9B1] via-[#D4AF37] to-[#B8941F] shadow-lg transform rotate-12 hover:rotate-6 transition-transform duration-300">
          {/* Inner shadow for depth */}
          <div className="absolute inset-1 rounded-full bg-gradient-to-tl from-[#B8941F]/30 to-transparent"></div>
          
          {/* Highlight for metallic effect */}
          <div className="absolute inset-2 rounded-full bg-gradient-to-br from-[#F4E9B1]/80 to-transparent transform -rotate-45"></div>
        </div>
        
        {/* Letter D with side glance effect */}
        <div className="relative z-10 font-bold text-[#0D1B2A] transform -skew-x-6 drop-shadow-sm">
          D
        </div>
        
        {/* Subtle glow effect */}
        <div className="absolute inset-0 rounded-full bg-gradient-to-br from-[#D4AF37]/20 to-transparent blur-sm animate-pulse"></div>
      </div>
    </div>
  );
}

// Alternative logo with text
export function DijaLogoWithText({ size = 'md', className = '' }: DijaLogoProps) {
  const sizeClasses = {
    sm: 'text-lg',
    md: 'text-xl',
    lg: 'text-3xl'
  };

  return (
    <div className={`flex items-center gap-3 ${className}`}>
      <DijaLogo size={size} />
      <div className="flex flex-col">
        <span className={`${sizeClasses[size]} font-bold text-[#0D1B2A] tracking-wide`}>
          DijaGold
        </span>
        <span className="text-xs text-[#D4AF37] font-medium tracking-wider uppercase">
          POS System
        </span>
      </div>
    </div>
  );
}

// Animated logo for loading states
export function DijaLogoAnimated({ size = 'md', className = '' }: DijaLogoProps) {
  const sizeClasses = {
    sm: 'h-8 w-8 text-2xl',
    md: 'h-10 w-10 text-3xl',
    lg: 'h-16 w-16 text-5xl'
  };

  return (
    <div className={`${sizeClasses[size]} ${className}`}>
      <div className="relative flex items-center justify-center h-full w-full animate-spin">
        {/* Rotating golden ring */}
        <div className="absolute inset-0 rounded-full border-4 border-transparent border-t-[#D4AF37] border-r-[#D4AF37] animate-spin"></div>
        
        {/* Inner logo */}
        <div className="relative z-10">
          <DijaLogo size={size} className="animate-none" />
        </div>
      </div>
    </div>
  );
}