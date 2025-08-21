import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Input } from './ui/input';
import { Button } from './ui/button';
import { Label } from './ui/label';
import { Badge } from './ui/badge';
import { Eye, EyeOff, Shield, User } from 'lucide-react';
import { useAuth } from './AuthContext';
import DijaLogo, { DijaLogoWithText, DijaLogoAnimated } from './DijaLogo';

export default function LoginScreen() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);
    console.log('Login form submitted with username:', username.trim());
    
    try {
      const success = await login(username.trim(), password);
      console.log('Login result:', success);
      
      if (!success) {
        console.log('Login failed - setting error message');
        setError('Invalid username or password');
      } else {
        console.log('Login successful - should redirect to main app');
      }
    } catch (err) {
      console.error('Login exception:', err);
      setError('Unable to sign in. Please try again.');
    } finally {
      setIsLoading(false);
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
              <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
                <p className="text-sm text-red-600">{error}</p>
              </div>
            )}

            {/* Login Button */}
            <Button
              type="submit"
              className="w-full h-12 text-lg bg-gradient-to-r from-[#D4AF37] to-[#B8941F] hover:from-[#B8941F] hover:to-[#D4AF37] text-[#0D1B2A] font-semibold shadow-lg transition-all duration-200 transform hover:scale-105"
              disabled={isLoading}
            >
              {isLoading ? (
                <div className="flex items-center gap-2">
                  <DijaLogoAnimated size="sm" />
                  Signing In...
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