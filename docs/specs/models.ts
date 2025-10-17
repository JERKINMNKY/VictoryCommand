// Auto-generated TypeScript models (subset). Useful for scaffolding and validation.
export interface ResourceMap {
  [key: string]: number
}

export interface Player {
  id: string
  username: string
  resources: ResourceMap
  premiumCurrency: number
}

export interface City {
  id: string
  ownerId?: string
  name: string
  level: number
  laborTotal: number
  laborIdle: number
}

export interface Building {
  id: string
  cityId?: string
  type: string
  level: number
}

export interface Unit {
  id: string
  ownerCityId?: string
  type: string
  tier: number
}
