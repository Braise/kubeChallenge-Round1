export interface Produit {
  id: string;
  nom: string;
  description: string;
  prix: number;
  image: string;
}
export interface ProduitVM {
  id: string;
  nom: string;
  description: string;
  prix: number;
  image: string;
  imageUrl: string;
  imageThumbUrl: string;
}
