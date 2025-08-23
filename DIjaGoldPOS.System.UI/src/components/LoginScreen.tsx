import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Input } from './ui/input';
import { Button } from './ui/button';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Eye, EyeOff, Shield, User, AlertCircle, Wifi, WifiOff } from 'lucide-react';
import { useAuth } from './AuthContext';
import DijaLogo, { DijaLogoWithText, DijaLogoAnimated } from './DijaLogo';
import { testApiConnection } from '../services/api';

export default function LoginScreen() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [errorType, setErrorType] = useState<'error' | 'warning' | 'info'>('error');
  const [isOnline, setIsOnline] = useState(true);
  const [isServerReachable, setIsServerReachable] = useState(true);
  
  const { login } = useAuth();

  // Check network connectivity and server reachability
  useEffect(() => {
    const checkConnectivity = async () => {
      // Check if browser is online
      setIsOnline(navigator.onLine);
      
      // Check if server is reachable
      try {
        const serverReachable = await testApiConnection();
        setIsServerReachable(serverReachable);
      } catch (error) {
        setIsServerReachable(false);
      }
    };

    checkConnectivity();

    // Listen for online/offline events
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  const getErrorMessage = (error: any): { message: string; type: 'error' | 'warning' | 'info' } => {
    // Handle network connectivity issues
    if (error instanceof TypeError && error.message.includes('fetch')) {
      return {
        message: 'Unable to connect to the server. Please check your internet connection and try again.',
        type: 'warning'
      };
    }

    // Handle HTTP status codes
    if (error.status) {
      switch (error.status) {
        case 400:
          return {
            /*
            message: 'Invalid request. Please check your input and try again.',
            */
            message:error.message,
            type: 'error'
          };
        case 401:
          return {
            //message: 'Invalid username or password. Please check your credentials and try again.',
            message:error.message,
            type: 'error'
          };
        case 403:
          return {
            //message: 'Access denied. Your account may be locked or you may not have permission to access this system.',
            message:error.message,
            type: 'warning'
          };
        case 404:
          return {
            //message: 'Login service not found. Please contact support.',
            message:error.message,
            type: 'error'
          };
        case 429:
          return {
            //message: 'Too many login attempts. Please wait a few minutes before trying again.',
            message:error.message,
            type: 'warning'
          };
        case 500:
          return {
            //message: 'Server error occurred. Please try again later or contact support.',
            message:error.message,
            type: 'error'
          };
        case 502:
        case 503:
        case 504:
          return {
            message: 'Service temporarily unavailable. Please try again later.',
            type: 'info'
          };
      }
    }

    /*
    // Handle specific API error messages
    if (error.message) {
      const message = error.message.toLowerCase();
      
      // Invalid credentials
      if (message.includes('invalid') || message.includes('incorrect') || message.includes('credentials')) {
        return {
          message: 'Invalid username or password. Please check your credentials and try again.',
          type: 'error'
        };
      }

      // Account locked or disabled
      if (message.includes('locked') || message.includes('disabled') || message.includes('suspended')) {
        return {
          message: 'Your account has been locked. Please contact your administrator.',
          type: 'warning'
        };
      }

      // Account expired
      if (message.includes('expired') || message.includes('expiration')) {
        return {
          message: 'Your account has expired. Please contact your administrator to renew your access.',
          type: 'warning'
        };
      }

      // Too many failed attempts
      if (message.includes('too many') || message.includes('attempts') || message.includes('rate limit')) {
        return {
          message: 'Too many failed login attempts. Please wait a few minutes before trying again.',
          type: 'warning'
        };
      }

      // Server errors
      if (message.includes('server') || message.includes('internal') || message.includes('500')) {
        return {
          message: 'Server error occurred. Please try again later or contact support.',
          type: 'error'
        };
      }

      // Maintenance mode
      if (message.includes('maintenance') || message.includes('unavailable') || message.includes('503')) {
        return {
          message: 'The system is currently under maintenance. Please try again later.',
          type: 'info'
        };
      }

      // Session expired
      if (message.includes('session') || message.includes('expired')) {
        return {
          message: 'Your session has expired. Please login again.',
          type: 'info'
        };
      }

      // Use the original error message if it's user-friendly
      if (message.length < 100) {
        return {
          message: error.message,
          type: 'error'
        };
      }
    }

  */

    // Default error message
    return {
      message: 'Unable to sign in. Please check your credentials and try again.',
      type: 'error'
    };
  };

  const retryConnectivityCheck = async () => {
    try {
      const serverReachable = await testApiConnection();
      setIsServerReachable(serverReachable);
      if (serverReachable) {
        setError('');
        setErrorType('error');
      }
    } catch (error) {
      setIsServerReachable(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setErrorType('error');
    setIsLoading(true);
    console.log('Login form submitted with username:', username.trim());
    
    try {
      await login(username.trim(), password);
      console.log('Login successful - user will be redirected automatically');
    } catch (err) {
      console.error('Login failed:', err);
      const { message, type } = getErrorMessage(err);
      setError(message);
      setErrorType(type);
      // No redirect on failure - user stays on login screen
    } finally {
      setIsLoading(false);
    }
  };

  const getErrorIcon = () => {
    switch (errorType) {
      case 'warning':
        return <AlertCircle className="h-4 w-4 text-amber-600" />;
      case 'info':
        return <AlertCircle className="h-4 w-4 text-blue-600" />;
      default:
        return <AlertCircle className="h-4 w-4 text-red-600" />;
    }
  };

  const getErrorStyles = () => {
    switch (errorType) {
      case 'warning':
        return 'bg-amber-50 border-amber-200 text-amber-800';
      case 'info':
        return 'bg-blue-50 border-blue-200 text-blue-800';
      default:
        return 'bg-red-50 border-red-200 text-red-600';
    }
  };
/*
  const quickLogin = (role: 'manager' | 'cashier') => {
    setUsername(role);
    // Demo credentials use a shared password defined in AuthContext mock: 'password'
    setPassword('password');
    setError('');
  };
*/
  return (
    <div className="min-h-screen bg-gradient-to-br from-[#0D1B2A] via-[#1A2B3D] to-[#0D1B2A] flex items-center justify-center p-4">
      {/* Background pattern */}
      <div className="absolute inset-0 opacity-5">
        <div className="absolute inset-0" style={{
          backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23D4AF37' fill-opacity='0.1'%3E%3Ccircle cx='30' cy='30' r='4'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
        }}></div>
      </div>

      <Card className="w-full max-w-md bg-white/95 backdrop-blur-sm border-0 shadow-2xl">
        <CardHeader className="text-center space-y-4 pb-8">
          {/* Logo */}
          <div className="flex justify-center">
            {isLoading ? (
              <DijaLogoAnimated size="lg" />
            ) : (
              <DijaLogoWithText size="lg" />
            )}
          </div>
          
          <div className="space-y-2">
            <CardTitle className="text-2xl text-[#0D1B2A]">
              Welcome Back
            </CardTitle>
            <CardDescription className="text-[#0D1B2A]/70">
              Sign in to your DijaGold POS account
            </CardDescription>
          </div>
        </CardHeader>

        <CardContent className="space-y-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Username Field */}
            <div className="space-y-2">
              <Label htmlFor="username" className="text-[#0D1B2A]">
                Username
              </Label>
              <div className="relative">
                <User className="absolute left-3 top-3 h-4 w-4 text-[#0D1B2A]/50" />
                <Input
                  id="username"
                  type="text"
                  placeholder="Enter your username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  className="pl-10 h-12 border-[#0D1B2A]/20 focus:border-[#D4AF37] focus:ring-[#D4AF37]"
                  disabled={isLoading}
                  required
                />
              </div>
            </div>

            {/* Password Field */}
            <div className="space-y-2">
              <Label htmlFor="password" className="text-[#0D1B2A]">
                Password
              </Label>
              <div className="relative">
                <Shield className="absolute left-3 top-3 h-4 w-4 text-[#0D1B2A]/50" />
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  placeholder="Enter your password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="pl-10 pr-10 h-12 border-[#0D1B2A]/20 focus:border-[#D4AF37] focus:ring-[#D4AF37]"
                  disabled={isLoading}
                  required
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="absolute right-0 top-0 h-12 px-3 hover:bg-transparent"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isLoading}
                >
                  {showPassword ? (
                    <EyeOff className="h-4 w-4 text-[#0D1B2A]/50" />
                  ) : (
                    <Eye className="h-4 w-4 text-[#0D1B2A]/50" />
                  )}
                </Button>
              </div>
            </div>

            {/* Error Message */}
            {error && (
              <div className={`p-3 border rounded-lg ${getErrorStyles()}`}>
                <div className="flex items-center gap-2">
                  {getErrorIcon()}
                  <p className="text-sm">{error}</p>
                </div>
              </div>
            )}

            {/* Connectivity Status */}
            {(!isOnline || !isServerReachable) && (
              <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <WifiOff className="h-4 w-4 text-amber-600" />
                    <div className="text-sm text-amber-800">
                      {!isOnline ? (
                        <span>You are currently offline. Please check your internet connection.</span>
                      ) : (
                        <span>Unable to reach the server. Please check your connection or try again later.</span>
                      )}
                    </div>
                  </div>
                  {!isOnline && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={retryConnectivityCheck}
                      className="text-amber-700 border-amber-300 hover:bg-amber-100"
                    >
                      Retry
                    </Button>
                  )}
                </div>
              </div>
            )}

            {/* Login Button */}
            <Button
              type="submit"
              className="w-full h-12 text-lg bg-gradient-to-r from-[#D4AF37] to-[#B8941F] hover:from-[#B8941F] hover:to-[#D4AF37] text-[#0D1B2A] font-semibold shadow-lg transition-all duration-200 transform hover:scale-105"
              disabled={isLoading || !isOnline || !isServerReachable}
            >
              {isLoading ? (
                <div className="flex items-center gap-2">
                  <DijaLogoAnimated size="sm" />
                  Signing In...
                </div>
              ) : !isOnline || !isServerReachable ? (
                <div className="flex items-center gap-2">
                  <WifiOff className="h-4 w-4" />
                  Connection Unavailable
                </div>
              ) : (
                'Sign In'
              )}
            </Button>
          </form>


          {/* Footer */}
          <div className="text-center pt-4 border-t border-[#0D1B2A]/10">
            <p className="text-xs text-[#0D1B2A]/60">
              DijaGold POS System v2.0.1
            </p>
            <p className="text-xs text-[#0D1B2A]/40 mt-1">
              © 2024 DijaGold Jewelry. All rights reserved.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}